using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    class AvcConfigurationRulesTests : AdminWebsiteUnitTestsBase
    {
        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private IWithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private BrandQueries _brandQueries;
        public BonusBalance Balance { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _withdrawalService = Container.Resolve<IWithdrawalService>();
            Balance = new BonusBalance();
            var bonusApiMock = new Mock<IBonusApiProxy>();
            bonusApiMock.Setup(proxy => proxy.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(Balance);
            Container.RegisterInstance(bonusApiMock.Object);
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            Container.Resolve<PaymentWorker>().Start();

            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            player.DateRegistered = DateTimeOffset.Now.AddMonths(-1);
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);
        }

        [Test]
        public void Withdrawal_goes_directly_to_acceptance_queue_after_exemption_applied()
        {
            var brandId = _brandQueries.GetBrands().First().Id;
            var licenseeId = _brandQueries.GetBrands().First().Licensee.Id;
            var currency = _brandQueries.GetBrands().First().DefaultCurrency;
            var vipLevelId = (Guid)_brandQueries.GetBrands().First().DefaultVipLevelId;
            var playerId = new Guid();

            var playersAffectedByAvc = _playerQueries.GetPlayers()
                .Where(player => player.BrandId == brandId
                                 && player.Brand.LicenseeId == licenseeId
                                 && player.CurrencyCode == currency
                                 && player.VipLevelId == vipLevelId);

            if (playersAffectedByAvc.Any())
                playerId = playersAffectedByAvc.FirstOrDefault().Id;

            //Create a new avc that has a withdrawal exemption 
            CreateAvcConfiguration(new AVCConfigurationDTO
            {
                Licensee = licenseeId,
                Brand = brandId,
                Currency = currency,
                VipLevels = new[] { vipLevelId },

                HasWithdrawalExemption = true,

                HasWithdrawalCount = true,
                TotalWithdrawalCountAmount = 85,
                TotalWithdrawalCountOperator = ComparisonEnum.Greater,

                Status = AutoVerificationCheckStatus.Active
            });

            _paymentTestHelper.MakeDeposit(playerId, 1000);
            Balance.Main = 1000;

            //Setup an exemption for a particular player that will make a withdrawal request later in the test case.
            var playerExemption = new Exemption
            {
                Exempt = true,
                ExemptFrom = DateTime.UtcNow.AddDays(-2).ToString(CultureInfo.InvariantCulture),
                ExemptTo = DateTime.UtcNow.AddDays(2).ToString(CultureInfo.InvariantCulture),
                ExemptLimit = 1,

                PlayerId = playerId
            };

            _withdrawalService.SaveExemption(playerExemption);

            //Create the offline withdrawal request
            MakeWithdrawalRequest(playerId, playersAffectedByAvc);

            var numberOfVerifiedWithdrawals = _withdrawalService
                .GetWithdrawalsVerified().Count(wd => wd.PlayerBankAccount.Player.Id == playerId);

            Assert.AreEqual(1, numberOfVerifiedWithdrawals);

            //Since we are allowed to only one exemption, this means that on the next withdrawal
            //we must have our withdrawal request in the verification queue but not in the acceptance queue
            MakeWithdrawalRequest(playerId, playersAffectedByAvc);
            Assert.AreEqual(1, numberOfVerifiedWithdrawals);

        }
        #region Helper methods
        private OfflineWithdrawResponse MakeWithdrawalRequest(Guid playerId, IQueryable<Core.Common.Data.Player.Player> playersAffectedByAvc)
        {
            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts.
                    Include(x => x.Player).
                    First(x => x.Player.Id == playerId)
                    .Id,
                Remarks = "rogi",
                RequestedBy = playersAffectedByAvc.FirstOrDefault().Username
            };

            return _withdrawalService.Request(offlineWithdrawalRequest);
        }

        private void CreateAvcConfiguration(AVCConfigurationDTO avcConfiguration)
        {
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            avcConfiguration.Id = Guid.NewGuid();
            _avcConfigurationCommands.Create(avcConfiguration);
        }
        #endregion
    }
}
