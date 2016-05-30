using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;
using PaymentSettings = AFT.RegoV2.Core.Payment.Data.PaymentSettings;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private IActorInfoProvider _actorInfoProvider;
        private IGameRepository _walletRepository;
        private IWithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private GamesTestHelper _gamesTestHelper;
        private IWagerConfigurationCommands _wageringConfigurationCommands;
        private Core.Common.Data.Player.Player _player;
        private IPaymentQueries _paymentQueries;
        public BonusBalance Balance { get; set; }
        #endregion

        #region Methods

        public override void BeforeEach()
        {
            base.BeforeEach();

            Balance = new BonusBalance();
            var bonusApiMock = new Mock<IBonusApiProxy>();
            bonusApiMock.Setup(proxy => proxy.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(Balance);
            Container.RegisterInstance(bonusApiMock.Object);

            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            _walletRepository = Container.Resolve<IGameRepository>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _wageringConfigurationCommands = Container.Resolve<IWagerConfigurationCommands>();
            _paymentQueries = Container.Resolve<IPaymentQueries>();
            
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            Container.Resolve<PaymentWorker>().Start();
            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);

            _player = _playerQueries.GetPlayers().ToList().First();
        }

        [Test]
        public async Task Can_create_OW_request()
        {
            _paymentTestHelper.MakeDeposit(_player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, _player.Id);
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
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            var requests = _paymentRepository.OfflineWithdraws.ToList();
            Assert.IsNotEmpty(requests);

            var withdrawLockBalance = _paymentQueries.GetWithdrawalLockBalance(_player.Id);
            withdrawLockBalance.Should().Be(1);

            _paymentTestHelper.AssertBalance(_player.Id
                ,total:10000,playable:9999,main:9999,free:9999,withdrawalLock:1);
        }

        [Test]
        public async Task Can_create_OW_with_1x_auto_wager_check()
        {
            //Make deposit
            _paymentTestHelper.MakeDeposit(_player.Id, 100);

            //Make bet
            await _gamesTestHelper.PlaceAndWinBet(20, 20,  _player.Id);
            await _gamesTestHelper.PlaceAndWinBet(30, 30,  _player.Id);
            await _gamesTestHelper.PlaceAndWinBet(50, 100, _player.Id);
            Balance.Main = 150;

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
            response.Should().NotBeNull();

            _paymentTestHelper.AssertBalance(_player.Id
                , total: 150, playable: 149, main: 149, free: 149, withdrawalLock: 1);
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "Deposit wagering requirement has not been completed.")]
        public async Task Can_create_OW_with_1x_auto_wager_check_with_gap_between_deposits()
        {
            var player = _player;

            var wagerId = _wageringConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO()
            {
                BrandId = player.BrandId,
                IsDepositWageringCheck = true,
                Currency = player.CurrencyCode
            }, Guid.NewGuid());
            _wageringConfigurationCommands.ActivateWagerConfiguration(wagerId, Guid.NewGuid());
            //Make deposit
            _paymentTestHelper.MakeDeposit(player.Id, 1000);

            //Make bet
            await _gamesTestHelper.PlaceAndWinBet(500, 600, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(300, 600, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(200, 600, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(500, 600, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(50,  600, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(300, 600, player.Id);
            Balance.Main = 2750;

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 200,
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

            //Make deposit
            _paymentTestHelper.MakeDeposit(player.Id, 500);

            //Make bet
            await _gamesTestHelper.PlaceAndWinBet(300, 600, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(100, 600, player.Id);
            Balance.Main = 4050;

            offlineWithdrawalRequest = new OfflineWithdrawRequest
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

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "{\"text\":\"app:payment.amountExceedsBalance\"}")]
        public void Cannot_create_OW_greater_than_limit_per_day()
        {
            var player = _player;

            _paymentTestHelper.MakeDeposit(player.Id, 30);
            Balance.Main = 30;

            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = player.BrandId,
                VipLevel = player.VipLevel.Id.ToString(),
                CurrencyCode = player.CurrencyCode,
                MinAmountPerTransaction = 100,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 30,
                MaxTransactionPerDay = 40,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 31,
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
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "{\"text\":\"app:payment.amountExceedsBalance\"}")]
        public void Cannot_create_OW_request_exceeding_transaction_limit()
        {
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = _player.BrandId,
                VipLevel = _player.VipLevel.Id.ToString(),
                CurrencyCode = _player.CurrencyCode,
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 20,
                MaxAmountPerDay = 30,
                MaxTransactionPerDay = 40,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 21,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "{\"text\": \"app:payment.settings.amountBelowAllowedValueError\", \"variables\": {\"value\": \"10.00\"}}")]
        public void Cannot_create_OW_lower_than_transaction_limit()
        {
            _paymentTestHelper.MakeDeposit(_player.Id);
            Balance.Main = 200;
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = _player.BrandId,
                VipLevel = _player.VipLevel.Id.ToString(),
                CurrencyCode = _player.CurrencyCode,
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 20,
                MaxAmountPerDay = 30,
                MaxTransactionPerDay = 40,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60,
                PaymentType = PaymentType.Withdraw
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "Deposit wagering requirement has not been completed.")]
        public async Task Cannot_create_OW_with_auto_wager_check()
        {
            var wagerId = _wageringConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO()
            {
                BrandId = _player.BrandId,
                IsDepositWageringCheck = true,
                Currency = _player.CurrencyCode
            }, Guid.NewGuid());
            _wageringConfigurationCommands.ActivateWagerConfiguration(wagerId, Guid.NewGuid());
            //Make deposit
            _paymentTestHelper.MakeDeposit(_player.Id, 200);
            //Make bet
            await _gamesTestHelper.PlaceAndWinBet(20, 10000, _player.Id);
            await _gamesTestHelper.PlaceAndWinBet(20, 10000, _player.Id);
            await _gamesTestHelper.PlaceAndWinBet(50, 10000, _player.Id);

            //Make one more deposit
            _paymentTestHelper.MakeDeposit(_player.Id, 100);

            //Bet one more
            await _gamesTestHelper.PlaceAndWinBet(20, 10000, _player.Id);
            Balance.Main = 40190;

            _walletRepository.SaveChanges();

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "{\"text\":\"app:payment.amountExceedsBalance\"}")]
        public void Cannot_create_OWs_more_than_transaction_limit_per_day()
        {
            const int transactionsPerDay = 10;
            
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = _player.BrandId,
                VipLevel = _player.VipLevel.Id.ToString(),
                CurrencyCode = _player.CurrencyCode,
                MinAmountPerTransaction = -10,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 100,
                MaxTransactionPerDay = transactionsPerDay,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            for (int i = 0; i < 10; i++)
            {
                var offlineWithdrawalRequest = new OfflineWithdrawRequest
                {
                    Amount = 1,
                    NotificationType = NotificationType.None,
                    BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                    BankTime = _paymentRepository.Banks.First().Created.ToString(),
                    PlayerBankAccountId = _paymentRepository
                        .PlayerBankAccounts
                        .Include(x => x.Player)
                        .First(x => x.Player.Id == _player.Id)
                        .Id,
                    Remarks = "asd",
                    RequestedBy = _actorInfoProvider.Actor.UserName
                };

                _withdrawalService.Request(offlineWithdrawalRequest);
            }

            var lastRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(lastRequest);
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "{\"text\":\"app:payment.amountExceedsBalance\"}")]
        public void Cannot_create_OWs_more_than_transaction_limit_per_month()
        {
            const int transactionsPerMonth = 10;
            var player = _player;
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = player.BrandId,
                VipLevel = player.VipLevel.Id.ToString(),
                CurrencyCode = player.CurrencyCode,
                MinAmountPerTransaction = -10,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 100,
                MaxTransactionPerDay = 100,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = transactionsPerMonth
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            for (int i = 0; i < 10; i++)
            {
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

            var lastRequest = new OfflineWithdrawRequest
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

            var response = _withdrawalService.Request(lastRequest);
        }

        [Test]
        [ExpectedException(typeof(RegoValidationException), ExpectedMessage = "{\"text\":\"app:payment.amountExceedsBalance\"}")]
        public void Cannot_create_OWs_more_than_transaction_limit_per_week()
        {
            const int transactionPerWeek = 10;
            var player = _player;
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = player.BrandId,
                VipLevel = player.VipLevel.Id.ToString(),
                CurrencyCode = player.CurrencyCode,
                MinAmountPerTransaction = -10,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 100,
                MaxTransactionPerDay = 100,
                MaxTransactionPerWeek = transactionPerWeek,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            for (int i = 0; i < 10; i++)
            {
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

            var lastRequest = new OfflineWithdrawRequest
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

            var response = _withdrawalService.Request(lastRequest);
        }

        [Test]
        public async Task Can_cancel_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName);
            
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9999, main: 9999, free: 9999, withdrawalLock: 1);

            var cancelRemarkts = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Cancel(response.Id, cancelRemarkts);
            var ow = _withdrawalService.GetWithdrawalsCanceled();

            Assert.IsNotEmpty(ow);
            Assert.AreEqual(1, ow.Count());

            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 10000, main: 10000, free: 10000, withdrawalLock: 0);

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Canceled);
            withdraw.CanceledTime.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.CanceledBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(cancelRemarkts);
        }

        [Test]
        public async Task Can_accept_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9999, main: 9999, free: 9999, withdrawalLock: 1);

            var acceptRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Accept(response.Id, acceptRemarks);
            var ow = _withdrawalService.GetWithdrawalsAccepted();
            Assert.IsNotEmpty(ow);
            Assert.AreEqual(1, ow.Count());

            //assert balance after Accept
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9999, main: 9999, free: 9999, withdrawalLock: 1);          

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Accepted);
            withdraw.AcceptedTime.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.AcceptedBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(acceptRemarks);
        }

        [Test]
        public async Task Can_approve_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9999, main: 9999, free: 9999, withdrawalLock: 1);

            //have to accept withdraw,before approve
            _withdrawalService.Accept(response.Id, TestDataGenerator.GetRandomString(10));
                               
            var approveRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Approve(response.Id, approveRemarks);         
           
            //assert balance after Approve
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 9999, playable: 9999, main: 9999, free: 9999, withdrawalLock: 0);

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Approved);
            withdraw.Approved.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.ApprovedBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(approveRemarks);
        }

        [Test]
        public async Task Can_unverify_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9999, main: 9999, free: 9999, withdrawalLock: 1);

            //have to revert withdraw,before unverify
            _withdrawalService.Revert(response.Id, TestDataGenerator.GetRandomString(10));

            var unverifyRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Unverify(response.Id, unverifyRemarks);

            //assert balance after Unverify
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 10000, main: 10000, free: 10000, withdrawalLock: 0);

            var withdraw= _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Unverified);
            withdraw.Unverified.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.UnverifiedBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(unverifyRemarks);
        }

        [Test]
        public async Task Can_revert_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName,100);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            //have to revert withdraw,before unverify
            var revertRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Revert(response.Id, revertRemarks);
            var ow = _withdrawalService.GetWithdrawalsForVerification();
            Assert.IsNotEmpty(ow);
            Assert.AreEqual(1, ow.Count());
        
            //assert balance after revert
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Reverted);
            withdraw.RevertedTime.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.RevertedBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(revertRemarks);
        }

        [Test]
        public async Task Can_verify_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName, 100);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            //have to revert withdraw,before verify
            var revertRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Revert(response.Id, revertRemarks);

            var verifyRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Verify(response.Id, verifyRemarks);

            //assert balance after verify
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Verified);
            withdraw.Verified.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.VerifiedBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(verifyRemarks);
        }

        [Test]
        public async Task Can_verify_investigation_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName, 100);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            //have to revert withdraw,before verify
            var revertRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Revert(response.Id, revertRemarks);

            var investigateRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.SetInvestigateState(response.Id, investigateRemarks);

            //assert balance after SetInvestigateState
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Investigation);
            withdraw.InvestigationDate.Should().Be(null);
            withdraw.InvestigatedBy.Should().BeNull();
            withdraw.Remarks.Should().Be(investigateRemarks);

            var verifyRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Verify(response.Id, verifyRemarks);
            
            withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Verified);
            withdraw.Verified.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.VerifiedBy.Should().Be(_actorInfoProvider.Actor.UserName);            
            withdraw.Remarks.Should().Be(verifyRemarks);
            withdraw.InvestigationDate.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.InvestigationStatus.Should().Be(CommonVerificationStatus.Passed);
            //withdraw.InvestigatedBy.Should().Be(_actorInfoProvider.Actor.UserName);//TBD:is not set in verify
        }

        [Test]
        public async Task Can_verify_documents_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id);
            Balance.Main = 10000;

            var response = _paymentTestHelper.MakeWithdraw(player.Id, _actorInfoProvider.Actor.UserName, 100);

            //assert balance after request
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            //have to revert withdraw,before verify
            var revertRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Revert(response.Id, revertRemarks);

            var investigateRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.SetDocumentsState(response.Id, investigateRemarks);

            //assert balance after SetDocumentsState
            _paymentTestHelper.AssertBalance(_player.Id
                , total: 10000, playable: 9900, main: 9900, free: 9900, withdrawalLock: 100);

            var withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Documents);
            withdraw.DocumentsCheckDate.Should().Be(null);
            withdraw.DocumentsCheckStatus.Should().BeNull();
            withdraw.Remarks.Should().Be(investigateRemarks);

            var verifyRemarks = TestDataGenerator.GetRandomString(10);
            _withdrawalService.Verify(response.Id, verifyRemarks);
            
            withdraw = _paymentRepository.OfflineWithdraws.FirstOrDefault(x => x.Id == response.Id);
            withdraw.Status.Should().Be(WithdrawalStatus.Verified);
            withdraw.Verified.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.VerifiedBy.Should().Be(_actorInfoProvider.Actor.UserName);
            withdraw.Remarks.Should().Be(verifyRemarks);
            withdraw.DocumentsCheckDate.Should().BeCloseTo(DateTimeOffset.Now, 60000);
            withdraw.DocumentsCheckStatus.Should().Be(CommonVerificationStatus.Passed);
        }
        #endregion
    }
}