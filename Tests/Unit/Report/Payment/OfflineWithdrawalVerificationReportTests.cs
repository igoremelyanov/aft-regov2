using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Report.Payment
{
    internal class OfflineWithdrawalVerificationReportTests : AdminWebsiteUnitTestsBase
    {
        #region Fields
        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private IWithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private Core.Brand.Interface.Data.Brand _observedBrand;
        private Guid _observedRiskLevel;
        private Guid _riskLevelDifferentThanObserved;
        private BrandQueries _brandQueries;
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private IRiskProfileCheckCommands _riskProfileCheckCommands;
        private IActorInfoProvider _actorInfoProvider;
        private GamesTestHelper _gamesTestHelper;
        private IWithdrawalVerificationLogsQueues _withdrawalVerificationLogsQueues;
        private IRiskLevelQueries _riskLevelQueries;
        public BonusBalance Balance { get; set; }
        #endregion

        public override void BeforeEach()
        {
            base.BeforeEach();

            Balance = new BonusBalance();
            var bonusApiMock = new Mock<IBonusApiProxy>();
            bonusApiMock.Setup(proxy => proxy.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(Balance);
            Container.RegisterInstance(bonusApiMock.Object);

            Container.Resolve<PaymentWorker>().Start();

            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _withdrawalVerificationLogsQueues = Container.Resolve<IWithdrawalVerificationLogsQueues>();
            _riskLevelQueries = Container.Resolve<IRiskLevelQueries>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            _brandQueries = Container.Resolve<BrandQueries>();

            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();

            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _riskProfileCheckCommands = Container.Resolve<IRiskProfileCheckCommands>();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);
            _observedBrand = _brandQueries.GetBrands().First();

            brandHelper.CreateRiskLevel(_observedBrand.Id);

            //Replace with risk levels from fraud repository
            var availableRiskLevels = _riskLevelQueries.GetByBrand(_observedBrand.Id);
            _observedRiskLevel = availableRiskLevels.FirstOrDefault().Id;

            _riskLevelDifferentThanObserved = availableRiskLevels.First(rl => rl.Id != _observedRiskLevel).Id;
        }

        #region Withdrawal count rule
        [TestCase(ComparisonEnum.Greater, FraudRiskLevelStatus.Low)]
        [TestCase(ComparisonEnum.Less, FraudRiskLevelStatus.High)]
        public async Task Withdrawal_count_rule_is_interpreted_in_oposite_manner_between_AVC_and_RPC(ComparisonEnum comparison, FraudRiskLevelStatus status)
        {
            var playerId = new Guid();

            playerId = PickAPlayer(playerId);

            CreateAvcConfiguration(new AVCConfigurationDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasWithdrawalCount = true,
                TotalWithdrawalCountAmount = 85,
                TotalWithdrawalCountOperator = ComparisonEnum.Greater,

                Status = AutoVerificationCheckStatus.Active
            });

            _riskProfileCheckCommands.Create(new RiskProfileCheckDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasWithdrawalCount = true,
                TotalWithdrawalCountAmount = 85,
                TotalWithdrawalCountOperator = comparison
            });

            await DepositBetAndWin(playerId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == playerId)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            AssertRiskProfileCheckStatus(offlineWithdrawalRequest,
                _observedBrand,
                status);
        }
        #endregion

        #region Account age rule
        [TestCase(ComparisonEnum.Less, FraudRiskLevelStatus.Low)]
        [TestCase(ComparisonEnum.GreaterOrEqual, FraudRiskLevelStatus.High)]
        public async Task Account_age_rule_is_interpreted_in_oposite_manner_between_AVC_and_RPC(ComparisonEnum comparison, FraudRiskLevelStatus status)
        {
            var playerId = new Guid();

            playerId = PickAPlayer(playerId);

            CreateAvcConfiguration(new AVCConfigurationDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasAccountAge = true,
                AccountAge = 0,
                AccountAgeOperator = ComparisonEnum.Less,

                Status = AutoVerificationCheckStatus.Active
            });

            _riskProfileCheckCommands.Create(new RiskProfileCheckDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasAccountAge = true,
                AccountAge = 0,
                AccountAgeOperator = comparison,
            });

            await DepositBetAndWin(playerId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == playerId)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            AssertRiskProfileCheckStatus(offlineWithdrawalRequest,
                _observedBrand,
                status);
        }
        #endregion

        #region Deposit count rule
        [TestCase(ComparisonEnum.Greater, FraudRiskLevelStatus.Low)]
        [TestCase(ComparisonEnum.Less, FraudRiskLevelStatus.High)]
        public async Task Deposit_count_rule_is_interpreted_in_oposite_manner_between_AVC_and_RPC(ComparisonEnum comparison, FraudRiskLevelStatus status)
        {
            var playerId = new Guid();

            playerId = PickAPlayer(playerId);

            CreateAvcConfiguration(new AVCConfigurationDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasDepositCount = true,
                TotalDepositCountAmount = 50,
                TotalDepositCountOperator = ComparisonEnum.Greater,

                Status = AutoVerificationCheckStatus.Active
            });

            _riskProfileCheckCommands.Create(new RiskProfileCheckDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasDepositCount = true,
                TotalDepositCountAmount = 50,
                TotalDepositCountOperator = comparison,
            });

            await DepositBetAndWin(playerId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == playerId)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            AssertRiskProfileCheckStatus(offlineWithdrawalRequest,
                _observedBrand,
                status);
        }
        #endregion

        #region Fraud risk level rule
        [Test]
        public async void Has_fraud_risk_level_rule_is_interpreted_in_oposite_manner_between_AVC_and_RPC()
        {
            await ValidateRiskLevelCriteriaCheck(_observedRiskLevel, FraudRiskLevelStatus.Low);

            //await ValidateRiskLevelCriteriaCheck(_riskLevelDifferentThanObserved, FraudRiskLevelStatus.High);
        }
        #endregion

        #region WinLoss rule
        [TestCase(ComparisonEnum.Greater, FraudRiskLevelStatus.Low)]
        [TestCase(ComparisonEnum.LessOrEqual, FraudRiskLevelStatus.High)]
        public async void Win_Loss_rule_is_interpreted_in_oposite_manner_between_AVC_and_RPC(ComparisonEnum comparison, FraudRiskLevelStatus status)
        {
            var playerId = new Guid();

            playerId = PickAPlayer(playerId);

            CreateAvcConfiguration(new AVCConfigurationDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasWinLoss = true,
                WinLossAmount = 5,
                WinLossOperator = ComparisonEnum.Greater,

                Status = AutoVerificationCheckStatus.Active
            });

            _riskProfileCheckCommands.Create(new RiskProfileCheckDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },

                HasWinLoss = true,
                WinLossAmount = 5,
                WinLossOperator = comparison,
            });

            await DepositBetAndWin(playerId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == playerId)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            AssertRiskProfileCheckStatus(offlineWithdrawalRequest,
                _observedBrand,
                status);
        }
        #endregion


        #region Private Methods
        private void CreateAvcConfiguration(AVCConfigurationDTO avcConfiguration)
        {
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            avcConfiguration.Id = Guid.NewGuid();
            _avcConfigurationCommands.Create(avcConfiguration);
        }

        private void AssertRiskProfileCheckStatus(OfflineWithdrawRequest offlineWithdrawalRequest, 
            Core.Brand.Interface.Data.Brand brand, FraudRiskLevelStatus expectedResult)
        {
            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            var riskProfileCheckReportInfo = _withdrawalVerificationLogsQueues.RiskProfileCheckStatus(response.Id,
                brand.Name, brand.Licensee.Name, "Random Name");

            Assert.AreEqual(expectedResult.ToString(),
                riskProfileCheckReportInfo.VerificationDialogHeaderValues.StatusSuccess);
        }

        private async Task DepositBetAndWin(Guid playerId)
        {
            _paymentTestHelper.MakeDeposit(playerId, 1000, waitForProcessing: true);
            Balance.Main = 1000;
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, playerId);
            Balance.Main = 10000;
        }

        private Guid PickAPlayer(Guid playerId)
        {
            var playersAffectedByAvc = _playerQueries.GetPlayers()
                .Where(player => player.BrandId == _observedBrand.Id
                                 && player.Brand.LicenseeId == _observedBrand.Licensee.Id
                                 && player.CurrencyCode == _observedBrand.DefaultCurrency
                                 && player.VipLevelId == _observedBrand.DefaultVipLevelId);

            if (playersAffectedByAvc.Any())
                playerId = playersAffectedByAvc.FirstOrDefault().Id;
            return playerId;
        }

        private async Task ValidateRiskLevelCriteriaCheck(Guid riskLevelForRpc, FraudRiskLevelStatus expectedResult)
        {
            var playerId = new Guid();

            playerId = PickAPlayer(playerId);

            CreateAvcConfiguration(new AVCConfigurationDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },
                HasFraudRiskLevel = true,
                RiskLevels = new[] { _observedRiskLevel },
                Status = AutoVerificationCheckStatus.Active
            });

            _riskProfileCheckCommands.Create(new RiskProfileCheckDTO
            {
                Licensee = _observedBrand.LicenseeId,
                Brand = _observedBrand.Id,
                Currency = _observedBrand.DefaultCurrency,
                VipLevels = new[] { (Guid)_observedBrand.DefaultVipLevelId },
                HasFraudRiskLevel = true,
                RiskLevels = new[] { riskLevelForRpc },
            });

            await DepositBetAndWin(playerId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == playerId)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            AssertRiskProfileCheckStatus(offlineWithdrawalRequest,
                _observedBrand,
                expectedResult);
        }
        #endregion
    }
}
