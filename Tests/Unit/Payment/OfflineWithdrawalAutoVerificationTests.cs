using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.Tests.Unit.Fraud;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalAutoVerificationTests : AdminWebsiteUnitTestsBase
    {
        private FakePaymentRepository _paymentRepository;
        private IActorInfoProvider _actorInfoProvider;
        private FakeGameRepository _gameRepository;
        private IWithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private GamesTestHelper _gamesTestHelper;
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private BrandQueries _brandQueries;
        private IFraudRepository _fraudRepository;
        public BonusBalance Balance { get; set; }
        public Core.Common.Data.Player.Player player { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            Balance = new BonusBalance();
            var bonusApiMock = new Mock<IBonusApiProxy>();
            bonusApiMock.Setup(proxy => proxy.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(Balance);
            Container.RegisterInstance(bonusApiMock.Object);

            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();

            _gameRepository = Container.Resolve<FakeGameRepository>();
            _gameRepository.SavedChanges += (s, e) =>
            {
                var allGameActions = _gameRepository.Rounds
                    .SelectMany(o => o.GameActions);

                foreach (var gameAction in allGameActions)
                {
                    if (Enumerable.Any(_gameRepository.GameActions, o => o.Id == gameAction.Id))
                        continue;

                    gameAction.Round.Game = _gameRepository.Games
                        .Single(o => o.Id == gameAction.Round.GameId);

                    _gameRepository.GameActions.Add(gameAction);
                }
            };

            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();
            _fraudRepository = Container.Resolve<IFraudRepository>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();
            Container.Resolve<RiskLevelWorker>().Start();
            Container.Resolve<PaymentWorker>().Start();
            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            var bankAccount = _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);
            bankAccount.Player.DateRegistered = DateTimeOffset.Now.AddMonths(-1);
        }

        [Test]
        public async Task Can_not_create_ow_with_age_greater_then_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasAccountAge = true,
                                AccountAge = 4,
                                AccountAgeOperator = ComparisonEnum.LessOrEqual,
                                Status = AutoVerificationCheckStatus.Active
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

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

            var accountAgeFailed = _fraudRepository.WithdrawalVerificationLogs.Any(
                o => o.VerificationStep == VerificationStep.AccountAge
                    && !o.IsSuccess);

            Assert.IsTrue(accountAgeFailed);
        }

        [Test]
        public async Task Can_create_ow_with_age_greater_then_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = false,
                                HasAccountAge = true,
                                AccountAge = 5,
                                AccountAgeOperator = ComparisonEnum.Greater
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

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

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public async Task Can_create_ow_with_deposit_count_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasDepositCount = true,
                                TotalDepositCountAmount = 1,
                                TotalDepositCountOperator = ComparisonEnum.Greater
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 11000;

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

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public async Task Can_not_create_ow_with_deposit_count_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasDepositCount = true,
                                TotalDepositCountAmount = 6,
                                TotalDepositCountOperator = ComparisonEnum.Greater,
                                Status = AutoVerificationCheckStatus.Active
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 11000;

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

            var depositCountFailed = _fraudRepository.WithdrawalVerificationLogs.Any(
                o => o.VerificationStep == VerificationStep.DepositCount
                    && !o.IsSuccess);

            Assert.IsTrue(depositCountFailed);
        }

        [Test]
        public async Task Can_create_ow_with_deposit_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = false,
                                HasTotalDepositAmount = true,
                                TotalDepositAmount = 500,
                                TotalDepositAmountOperator = ComparisonEnum.Greater
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

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

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            Assert.IsNotNull(response.Id);
        }


        [Test]
        public async Task Can_create_ow_with_total_withdrawal_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWithdrawalCount = true,
                                TotalWithdrawalCountAmount = 1,
                                TotalWithdrawalCountOperator = ComparisonEnum.Greater
                            }
                );

            _paymentRepository.OfflineWithdraws.Add(new OfflineWithdraw
            {
                Id = Guid.NewGuid(),
                Amount = 20,
                Status = WithdrawalStatus.Approved,
                PlayerBankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).First(x => x.Player.Id == player.Id)
            });
            _paymentRepository.OfflineWithdraws.Add(new OfflineWithdraw
            {
                Id = Guid.NewGuid(),
                Amount = 20,
                Status = WithdrawalStatus.Approved,
                PlayerBankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).First(x => x.Player.Id == player.Id)
            });

            _paymentRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;
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

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public async Task Can_not_create_ow_with_total_withdrawal_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWithdrawalCount = true,
                                TotalWithdrawalCountAmount = 10,
                                TotalWithdrawalCountOperator = ComparisonEnum.Greater,
                                Status = AutoVerificationCheckStatus.Active
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;
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

            var withdrawalCountFailed = _fraudRepository.WithdrawalVerificationLogs.Any(
                o => o.VerificationStep == VerificationStep.WithdrawalCount
                    && !o.IsSuccess);

            Assert.IsTrue(withdrawalCountFailed);
        }

        [Test]
        public async Task Can_create_ow_with_winloss_rule_less_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinLoss = true,
                                WinLossAmount = 100,
                                WinLossOperator = ComparisonEnum.Greater
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10, player.Id);
            Balance.Main = 10;
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

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public async Task Can_not_create_ow_with_winloss_rule_less_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinLoss = true,
                                WinLossAmount = 1000,
                                WinLossOperator = ComparisonEnum.Greater,
                                Status = AutoVerificationCheckStatus.Active
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            Balance.Main = 1000;
            await _gamesTestHelper.PlaceAndWinBet(1000, 10, player.Id);
            Balance.Main = 10;
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

            var winLossFailed = _fraudRepository.WithdrawalVerificationLogs.Any(
                o => o.VerificationStep == VerificationStep.WinLoss
                    && !o.IsSuccess);

            Assert.IsTrue(winLossFailed);
        }

        [Test]
        public async Task Can_not_create_ow_with_deposit_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                VipLevels = new[] { brand.DefaultVipLevelId.Value },
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasTotalDepositAmount = true,
                                TotalDepositAmount = 10000,
                                TotalDepositAmountOperator = ComparisonEnum.Greater,
                                Status = AutoVerificationCheckStatus.Active
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

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

            var depositAmountFailed = _fraudRepository.WithdrawalVerificationLogs.Any(
                o => o.VerificationStep == VerificationStep.TotalDepositAmount
                    && !o.IsSuccess);

            Assert.IsTrue(depositAmountFailed);
        }

        [Test]
        public async Task Can_not_make_ow_when_has_winning_amount_less_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            var externalGameId = _gamesTestHelper.GetMainWalletExternalGameId(player.Id);
            var providerId = _gameRepository.Games.Single(x => x.ExternalId == externalGameId).GameProviderId;

            CreateAvcConfiguration(
                new AVCConfigurationDTO
                {
                    Brand = brand.Id,
                    VipLevels = new[] { brand.DefaultVipLevelId.Value },
                    Currency = brand.DefaultCurrency,
                    HasFraudRiskLevel = false,
                    HasWinnings = true,
                    WinningRules = new List<WinningRuleDTO>
                    {
                        FraudTestDataHelper.GenerateWinningRule(providerId)
                    },
                    Status = AutoVerificationCheckStatus.Active
                }
            );

            _paymentTestHelper.MakeDeposit(player.Id, 100);

            await _gamesTestHelper.PlaceAndWinBet(100, 99, player.Id);
            Balance.Main = 99;

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
                Remarks = "rogi",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            _withdrawalService.Request(offlineWithdrawalRequest);

            var isHasWinningsFailed = _fraudRepository.WithdrawalVerificationLogs.Any(
                o => o.VerificationStep == VerificationStep.HasWinnings
                    && !o.IsSuccess);

            Assert.IsTrue(isHasWinningsFailed);
        }

        [Test]
        public async Task Can_make_ow_when_has_winning_amount_greater_than_setted_up()
        {
            var externalGameId = _gamesTestHelper.GetMainWalletExternalGameId(player.Id);
            var providerId = _gameRepository.Games.Single(x => x.ExternalId == externalGameId).GameProviderId;

            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = true,
                                WinningRules = new List<WinningRuleDTO>
                                {
                                    FraudTestDataHelper.GenerateWinningRule(providerId)
                                },
                            }
                );

            _paymentTestHelper.MakeDeposit(player.Id, 101);
            await _gamesTestHelper.PlaceAndWinBet(101, 101, player.Id);
            Balance.Main = 101;

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                   .PlayerBankAccounts.
                   Include(x => x.Player).
                   First(x => x.Player.Id == player.Id)
                   .Id,
                Remarks = "rogi",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            _withdrawalService.Request(offlineWithdrawalRequest);

            var isSuccess = _fraudRepository.WithdrawalVerificationLogs.All(
                o => o.IsSuccess);

            Assert.IsTrue(isSuccess);
        }

        private void CreateAvcConfiguration(AVCConfigurationDTO avcConfiguration)
        {
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            avcConfiguration.Id = Guid.NewGuid();
            _avcConfigurationCommands.Create(avcConfiguration);
        }
    }
}
