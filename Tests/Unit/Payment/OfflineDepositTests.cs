using System;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class OfflineDepositTests : AdminWebsiteUnitTestsBase
    {
        private IOfflineDepositCommands _commandsHandler;
        private IPaymentRepository _paymentRepository;

        private Core.Payment.Data.Player _player;

        private BankAccount _bankAccount;

        private PaymentTestHelper _paymentTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            Container.Resolve<PaymentWorker>().Start();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            _paymentRepository = Container.Resolve<IPaymentRepository>();
            _paymentRepository.Countries.Add(new Country { Code = "UA", Name = "Ukraine" });

            var playerTestHelper = Container.Resolve<PlayerTestHelper>();
            var player = playerTestHelper.CreatePlayer(brandId: brand.Id);
            _player = _paymentRepository.Players.Single(x => x.Id == player.Id);

            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(brand.Id, "CAD");
            _paymentTestHelper.CreatePlayerPaymentLevel(_player.Id, paymentLevel);

            var busMock = new Mock<IEventBus>();
            var offlineDepositValidator = new Mock<IPaymentSettingsValidator>();
            var actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            var playerIdentityValidator = new Mock<IPlayerIdentityValidator>();
            playerIdentityValidator.Setup(o => o.Validate(It.IsAny<Guid>(), It.IsAny<Core.Common.Data.Player.TransactionType>()));
            var docService = new Mock<IDocumentService>().Object;

            _commandsHandler = new OfflineDepositCommands(
                _paymentRepository,
                Container.Resolve<IPaymentQueries>(),
                busMock.Object,
                offlineDepositValidator.Object,
                actorInfoProvider,
                playerIdentityValidator.Object,
                docService,
                Container.Resolve<IOfflineDepositQueries>(),
                Container.Resolve<IServiceBus>(),
                Container.Resolve<IBonusApiProxy>(),
                Container.Resolve<IMessageTemplateService>());

            _bankAccount = _paymentTestHelper.CreateBankAccount(brand.Id, player.CurrencyCode);

            offlineDepositValidator
                .Setup(o => o.Validate(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Callback((Guid param1, string param2, decimal param3) => { });
        }

        [Test]
        public void Submit_command_should_throw_exception_if_player_not_found()
        {
            // Act
            Func<Task> act = async () => await _commandsHandler.Submit(new OfflineDepositRequest());

            // Assert
            act.ShouldThrow<RegoException>().Where(m => m.Message.Contains("Player not found"));
        }

        [Test]
        public void Submit_command_should_throw_exception_if_bank_account_not_found()
        {
            // Arrange
            //_playerQueriesMock.Setup(x => x.GetPlayer(_player.Id)).Returns(_player);

            // Act
            Func<Task> act = async () => await _commandsHandler.Submit(new OfflineDepositRequest { PlayerId = _player.Id });

            // Assert
            act.ShouldThrow<RegoException>().Where(m => m.Message.Contains("bankAccountNotFound"));
        }

        [Test]
        public void Submit_command_should_throw_exception_if_bank_account_and_player_accounts_have_different_currencies()
        {
            // Arrange
            _bankAccount.CurrencyCode = "UAH";

            // Act
            Func<Task> act = async () => await _commandsHandler.Submit(new OfflineDepositRequest { PlayerId = _player.Id, BankAccountId = _bankAccount.Id });

            // Assert
            act.ShouldThrow<RegoException>().Where(m => m.Message.Contains("differentCurrenciesErrorMessage"));
        }

        [Test]
        public async void Submit_command_should_create_new_offline_deposit()
        {
            // Act
            var offlineDeposit = await _commandsHandler.Submit(new OfflineDepositRequest
            {
                PlayerId = _player.Id,
                BankAccountId = _bankAccount.Id,
                Amount = 1020.3M
            });

            // Assert
            offlineDeposit.Id.Should().NotBeEmpty();
            offlineDeposit.Created.Should().BeCloseTo(DateTime.Now, 1000);
            offlineDeposit.TransactionNumber.Should().NotBeEmpty();
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.New);
            offlineDeposit.PaymentMethod.ShouldBeEquivalentTo(PaymentMethod.OfflineBank);
            offlineDeposit.DepositType.ShouldBeEquivalentTo(DepositType.Offline);
            offlineDeposit.Amount.ShouldBeEquivalentTo(1020.3M);
            offlineDeposit.BrandId.ShouldBeEquivalentTo(_player.BrandId);
            offlineDeposit.PlayerId.ShouldBeEquivalentTo(_player.Id);
            offlineDeposit.BankAccountId.ShouldBeEquivalentTo(_bankAccount.Id);
            offlineDeposit.CurrencyCode.ShouldBeEquivalentTo(_bankAccount.CurrencyCode);
        }

        [Test]
        [Ignore("Until Sergey Skripnik fix it, workflow changed")]
        public async void Confirm_command_requires_copies_of_player_ids_if_player_and_account_holder_names_are_different()
        {
            // Arrange
            var offlineDeposit = await GetNewOfflineDeposit("OD999888777");
            var offlineDepositConfirm = new OfflineDepositConfirm
            {
                Id = offlineDeposit.Id,
                PlayerAccountName = "Test"
            };

            // Act
            Action act0 = () => _commandsHandler.Confirm(offlineDepositConfirm, "", null, null, null);
            Action act1 = () => _commandsHandler.Confirm(offlineDepositConfirm, "", new byte[1], null, null);
            Action act2 = () => _commandsHandler.Confirm(offlineDepositConfirm, "", null, new byte[1], null);

            // Assert
            act0.ShouldThrow<ArgumentException>().WithMessage("Front and back copy of ID or receipt should be uploaded.");
            act1.ShouldThrow<ArgumentException>().WithMessage("Front and back copy of ID or receipt should be uploaded.");
            act2.ShouldThrow<ArgumentException>().WithMessage("Front and back copy of ID or receipt should be uploaded.");
        }

        [Test]
        public async void Confirm_offline_deposit()
        {
            // Arrange
            var offlineDeposit = await GetNewOfflineDeposit("OD12345678");
            var depositConfirm = new OfflineDepositConfirm
            {
                Id = offlineDeposit.Id,
                PlayerAccountName = "Fry Philip",
                PlayerAccountNumber = "Test PlayerAccountName",
                ReferenceNumber = "Test PlayerAccountName",
                Amount = 2345.56M,
                TransferType = Core.Payment.Interface.Data.TransferType.DifferentBank,
                OfflineDepositType = Core.Payment.Interface.Data.DepositMethod.ATM
            };

            // Act
            _commandsHandler.Confirm(depositConfirm, "", null, null, null);

            // Assert
            offlineDeposit.PlayerAccountName.ShouldBeEquivalentTo(depositConfirm.PlayerAccountName);
            offlineDeposit.PlayerAccountNumber.ShouldBeEquivalentTo(depositConfirm.PlayerAccountNumber);
            offlineDeposit.BankReferenceNumber.ShouldBeEquivalentTo(depositConfirm.ReferenceNumber);
            offlineDeposit.Amount.ShouldBeEquivalentTo(depositConfirm.Amount);
            offlineDeposit.TransferType.ShouldBeEquivalentTo(depositConfirm.TransferType);
            offlineDeposit.DepositMethod.ShouldBeEquivalentTo(depositConfirm.OfflineDepositType);
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Processing);
        }

        [Test]
        public void MyTest()
        {
            var offlineDeposit = _paymentTestHelper.CreateOfflineDeposit(_player.Id, 10000M);
            _paymentTestHelper.ConfirmOfflineDeposit(offlineDeposit);
            _paymentTestHelper.VerifyOfflineDeposit(offlineDeposit, true);

            var offlineDepositApprove = new OfflineDepositApprove
            {
                Id = offlineDeposit.Id,
                ActualAmount = 9988.77M,
                Fee = 10.50M,
                PlayerRemark = "Player remark",
                Remark = "Approve remark"
            };

            // Act
            _commandsHandler.Approve(offlineDepositApprove);

            var gameRepository = Container.Resolve<IGameRepository>();

            Assert.AreEqual(gameRepository.Wallets.Single().Balance, 9999.27M);
        }

        [Test]
        public async void Verify_offline_deposit()
        {
            // Arrange
            var offlineDeposit = await GetNewOfflineDeposit("OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, "", null, null, null);

            // Act
            _commandsHandler.Verify(offlineDeposit.Id, offlineDeposit.BankAccountId, "Verify remark");

            // Assert
            offlineDeposit.Remark.ShouldBeEquivalentTo("Verify remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Verified);
        }

        [Test]
        public async void Unverify_offline_deposit()
        {
            // Arrange
            var offlineDeposit = await GetNewOfflineDeposit("OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, "", null, null, null);

            // Act
            _commandsHandler.Unverify(offlineDeposit.Id, "Unverify remark", UnverifyReasons.D0001);

            // Assert
            offlineDeposit.Remark.ShouldBeEquivalentTo("Unverify remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Unverified);
        }


        [Test]
        public void Approve_offline_deposit()
        {
            // Arrange
            var offlineDeposit = _paymentTestHelper.CreateOfflineDeposit(_player.Id, 10000M);
            _paymentTestHelper.ConfirmOfflineDeposit(offlineDeposit);
            _paymentTestHelper.VerifyOfflineDeposit(offlineDeposit, true);

            var offlineDepositApprove = new OfflineDepositApprove
            {
                Id = offlineDeposit.Id,
                ActualAmount = 9988.77M,
                Fee = 10.50M,
                PlayerRemark = "Player remark",
                Remark = "Approve remark"
            };


            // Act
            _commandsHandler.Approve(offlineDepositApprove);

            // Assert
            offlineDeposit.ActualAmount.ShouldBeEquivalentTo(9988.77M);
            offlineDeposit.Fee.ShouldBeEquivalentTo(10.50M);
            offlineDeposit.PlayerRemark.ShouldBeEquivalentTo("Player remark");
            offlineDeposit.Remark.ShouldBeEquivalentTo("Approve remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Approved);
        }


        [Test]
        public async void Reject_offline_deposit()
        {
            // Arrange
            var offlineDeposit = await GetNewOfflineDeposit("OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, "", null, null, null);
            _commandsHandler.Verify(offlineDeposit.Id, offlineDeposit.BankAccountId, "Verify remark");

            // Act
            _commandsHandler.Reject(offlineDeposit.Id, "Reject remark");

            // Assert
            offlineDeposit.Remark.ShouldBeEquivalentTo("Reject remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Rejected);
        }

        [Test]
        public async void Confirm_offline_deposit_with_copies_of_player_ids()
        {
            // Arrange
            var offlineDeposit = await GetNewOfflineDeposit("OD12345678");
            var depositConfirm = new OfflineDepositConfirm
            {
                Id = offlineDeposit.Id,
                PlayerAccountName = "Test PlayerAccountName",
                PlayerAccountNumber = "Test PlayerAccountName",
                ReferenceNumber = "Test PlayerAccountName",
                Amount = 2345.56M,
                TransferType = Core.Payment.Interface.Data.TransferType.DifferentBank,
                OfflineDepositType = Core.Payment.Interface.Data.DepositMethod.ATM,
                IdFrontImage = "Test IdFrontImage.png",
                IdBackImage = "Test IdBackImage.jpg"
            };

            // Act
            _commandsHandler.Confirm(depositConfirm, "", new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, null);

            // Assert
            offlineDeposit.PlayerAccountName.ShouldBeEquivalentTo(depositConfirm.PlayerAccountName);
            offlineDeposit.PlayerAccountNumber.ShouldBeEquivalentTo(depositConfirm.PlayerAccountNumber);
            offlineDeposit.BankReferenceNumber.ShouldBeEquivalentTo(depositConfirm.ReferenceNumber);
            offlineDeposit.Amount.ShouldBeEquivalentTo(depositConfirm.Amount);
            offlineDeposit.TransferType.ShouldBeEquivalentTo(depositConfirm.TransferType);
            offlineDeposit.DepositMethod.ShouldBeEquivalentTo(depositConfirm.OfflineDepositType);
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Processing);
        }

        private async Task<OfflineDeposit> GetNewOfflineDeposit(string transactionNumber)
        {
            var offlineDeposit = await _commandsHandler.Submit(new OfflineDepositRequest
            {
                PlayerId = _player.Id,
                BankAccountId = _bankAccount.Id,
                Amount = 1020.3M
            });

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.Id == offlineDeposit.Id);
            deposit.TransactionNumber = transactionNumber;

            deposit.Player = new Core.Payment.Data.Player
            {
                Id = _player.Id,
                Username = _player.Username,
                FirstName = _player.FirstName,
                LastName = _player.LastName,
                BrandId = _player.BrandId,
                Brand = _player.Brand,
            };

            deposit.CurrencyCode = "CAD";

            return deposit;
        }
    }
}
