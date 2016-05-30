using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class PaymentPermissionsTests : PermissionsTestsBase
    {
        private IPaymentQueries _paymentQueries;
        private readonly Guid DefaultBrandId = Guid.Parse("00000000-0000-0000-0000-000000000138");

        public override void BeforeEach()
        {
            base.BeforeEach();

            _paymentQueries = Container.Resolve<IPaymentQueries>();
            var _paymentRepository = Container.Resolve<FakePaymentRepository>();

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
        public void Cannot_execute_PaymentQueries_without_permissions()
        {
            // Arrange
            LogWithNewAdmin(Modules.OfflineDepositRequests, Permissions.Create);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForConfirmation(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForViewRequest(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPaymentSettings());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPaymentLevelsAsQueryable());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetTransferSettings());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerForNewBankAccount(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetOfflineDeposits());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerBankAccounts());
        }

        [Test]
        public void Cannot_get_deposit_by_id_for_view_request_with_invalid_brand()
        {
            // Arrange
            var offlineDepositId = CreateNewOfflineDeposit();
            LogWithNewAdmin(Modules.OfflineDepositRequests, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForViewRequest(offlineDepositId));
        }

        [Test]
        public void Cannot_get_deposit_by_id_for_confirmation_with_invalid_brand()
        {
            // Arrange
            var offlineDepositId = CreateNewOfflineDeposit();
            LogWithNewAdmin(Modules.OfflineDepositConfirmation, Permissions.Confirm);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForConfirmation(offlineDepositId));
        }

        [Test]
        public void Cannot_get_player_for_new_bank_account_with_invalid_brand()
        {
            // Arrange
            var playerId = CreateNewPlayerBankAccount().Player.Id;
            LogWithNewAdmin(Modules.PlayerBankAccount, Permissions.Create);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerForNewBankAccount(playerId));
        }

        private Guid CreateNewOfflineDeposit()
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerId = CreateNewPlayer();
            var offlineDeposit = paymentTestHelper.CreateOfflineDeposit(playerId, 1M);

            return offlineDeposit.Id;
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

        private PlayerBankAccount CreateNewPlayerBankAccount()
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerId = CreateNewPlayer();
            var playerBankAccount = paymentTestHelper.CreatePlayerBankAccount(playerId, DefaultBrandId);


            return playerBankAccount;
        }
    }
}