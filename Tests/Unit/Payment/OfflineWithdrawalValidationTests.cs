using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalValidationTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private GamesTestHelper _gamesTestHelper;
        private FakePaymentRepository _paymentRepository;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerQueries _playerQueries;
        private IActorInfoProvider _actorInfoProvider;
        private IWithdrawalService _withdrawalService;
        private Mock<IAWCValidationService> mockAwcChec;
        private Mock<IBonusWageringWithdrawalValidationService> mockBonusWageringCheck;
        private Mock<IFundsValidationService> mockFundsValidationCheck;
        private Mock<IManualAdjustmentWageringValidationService> mockManualAdjWageringCheck;
        private Mock<IPaymentSettingsValidationService> mockPaymentSettingsCheck;
        private Mock<IRebateWageringValidationService> mockRebateValidationCheck;
        private Core.Common.Data.Player.Player _player;

        #endregion

        #region Methods

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            mockAwcChec = new Mock<IAWCValidationService>();
            Container.RegisterInstance(mockAwcChec.Object);
            mockBonusWageringCheck = new Mock<IBonusWageringWithdrawalValidationService>();
            Container.RegisterInstance(mockBonusWageringCheck.Object);
            mockPaymentSettingsCheck = new Mock<IPaymentSettingsValidationService>();
            Container.RegisterInstance(mockPaymentSettingsCheck.Object);
            mockManualAdjWageringCheck = new Mock<IManualAdjustmentWageringValidationService>();
            Container.RegisterInstance(mockManualAdjWageringCheck.Object);
            mockRebateValidationCheck = new Mock<IRebateWageringValidationService>();
            Container.RegisterInstance(mockRebateValidationCheck.Object);
            mockFundsValidationCheck = new Mock<IFundsValidationService>();
            Container.RegisterInstance(mockFundsValidationCheck.Object);
            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            Container.Resolve<PaymentWorker>().Start();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);

            _player = _playerQueries.GetPlayers().ToList().First();
        }

        [Test]
        public async Task Withdrawal_request_runs_auto_wager_check_validation()
        {
            await CreateOWR();
            mockAwcChec.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public async Task Withdrawal_request_runs_bonus_wagering_check_validation()
        {
            await CreateOWR();
            mockBonusWageringCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public async Task Withdrawal_request_runs_funds_check_validation()
        {
            await CreateOWR();
            mockFundsValidationCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public async Task Withdrawal_request_runs_manual_adj_wagering_check_validation()
        {
            await CreateOWR();
            mockManualAdjWageringCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public async Task Withdrawal_request_runs_payment_settings_check_validation()
        {
            await CreateOWR();
            mockPaymentSettingsCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public async Task Withdrawal_request_runs_rebate_wagering_check_validation()
        {
            await CreateOWR();
            mockRebateValidationCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        private async Task CreateOWR()
        {
            var player = _player;
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };
            _withdrawalService.Request(offlineWithdrawalRequest);
        }

        #endregion
    }
}