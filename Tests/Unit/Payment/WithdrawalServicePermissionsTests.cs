using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class WithdrawalServicePermissionsTests : PermissionsTestsBase
    {
        private IWithdrawalService _withdrawalService;
        private FakePaymentRepository _paymentRepository;
        private IActorInfoProvider _actorInfoProvider;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        public BonusBalance Balance { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            Balance = new BonusBalance();
            var bonusApiMock = new Mock<IBonusApiProxy>();
            bonusApiMock.Setup(proxy => proxy.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(Balance);
            Container.RegisterInstance(bonusApiMock.Object);

            Container.Resolve<PaymentWorker>().Start();
            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();

            _paymentRepository.Brands.Add(new Core.Payment.Data.Brand
            {
                Id = DefaultBrandId,
                Name = DefaultBrandId.ToString(),
                Code = "138",
                TimezoneId = "Pacific Standard Time"
            });

            _paymentRepository.Banks.Add(new Bank
            {
                Id = Guid.NewGuid(),
                BrandId = DefaultBrandId,
                BankName = "138Bank"
            });
        }

        protected override IEnumerable<Guid> GetAllowedAdminBrands()
        {
            return new List<Guid> { DefaultBrandId };
        }

        [Test]
        public void Cannot_execute_WithdrawalService_without_permissions()
        {
            // Arrange
            LogWithNewAdmin(Modules.OfflineDepositRequests, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsForVerification());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsForAcceptance());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsForApproval());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsCanceled());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsFailedAutoWagerCheck());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsOnHold());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Request(new OfflineWithdrawRequest()));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Verify(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Unverify(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Approve(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Reject(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassWager(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailWager(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassInvestigation(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailInvestigation(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Accept(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Revert(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Cancel(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.SaveExemption(new Exemption()));
        }

        [Test]
        public void Cannot_request_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var playerBankAccount = CreateNewPlayerBankAccount(DefaultBrandId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = playerBankAccount.Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            LogWithNewAdmin(Modules.OfflineWithdrawalRequest, Permissions.Create);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test]
        public void Cannot_verify_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalVerification, Permissions.Verify);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Verify(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_unverify_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalVerification, Permissions.Unverify);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Unverify(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_approve_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalApproval, Permissions.Approve);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Approve(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_reject_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalApproval, Permissions.Reject);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Reject(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_pass_wager_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalWagerCheck, Permissions.Pass);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassWager(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_fail_wager_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalWagerCheck, Permissions.Fail);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailWager(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_pass_investigation_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalInvestigation, Permissions.Pass);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassInvestigation(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_fail_investigation_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalInvestigation, Permissions.Fail);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailInvestigation(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_accept_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalAcceptance, Permissions.Accept);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Accept(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_revert_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalAcceptance, Permissions.Revert);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Revert(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_cancel_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewAdmin(Modules.OfflineWithdrawalAcceptance, Permissions.Revert);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Cancel(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_save_exemption_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var playerBankAccount = CreateNewPlayerBankAccount(DefaultBrandId);
            var exemption = new Exemption
            {
                PlayerId = playerBankAccount.Player.Id,
                Exempt = true,
                ExemptFrom = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExemptTo = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExemptLimit = 1
            };

            LogWithNewAdmin(Modules.OfflineWithdrawalExemption, Permissions.Exempt);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.SaveExemption(exemption));
        }

        private Guid CreateNewPlayer()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var player = playerTestHelper.CreatePlayer(true, brand.Id);

            return player.Id;
        }

        private PlayerBankAccount CreateNewPlayerBankAccount(Guid brandId)
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerId = CreateNewPlayer();
            var playerBankAccount = paymentTestHelper.CreatePlayerBankAccount(playerId, brandId);


            return playerBankAccount;
        }

        private Guid CreateOfflineWithdraw()
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerBankAccount = CreateNewPlayerBankAccount(DefaultBrandId);
            paymentTestHelper.MakeDeposit(playerBankAccount.Player.Id, 1000);
            Balance.Main = 1000;

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = playerBankAccount.Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var offlineWithdrawalResponse = _withdrawalService.Request(offlineWithdrawalRequest);

            return offlineWithdrawalResponse.Id;
        }
    }
}