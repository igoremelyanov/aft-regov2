using System;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class PlayerDepositVerifyTests : SeleniumBaseForAdminWebsite
    {
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");

        private const decimal Amount = 85;
        private DashboardPage _dashboardPage;
        private PlayerTestHelper _playerTestHelper;
        private PaymentTestHelper _paymentTestHelper;

        [Test]
        public void Can_unverify_deposit()
        {
            // create a player
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();

            var player = _playerTestHelper.CreatePlayer(true, DefaultBrandId);
            
            // create and confirm offline deposit
            var deposit = _paymentTestHelper.CreateOfflineDeposit(player.Id, Amount);
            _paymentTestHelper.ConfirmOfflineDeposit(deposit);
            var referenceCode = deposit.TransactionNumber;
            //var referenceCode = deposit.BankReferenceNumber;

            // unverify offline deposit
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerDepositVerifyPage = _dashboardPage.Menu.ClickPlayerDepositVerifyItem();
            playerDepositVerifyPage.FilterGrid(player.Username);
            playerDepositVerifyPage.SelectConfirmedDeposit(referenceCode);
            var unverifyForm = playerDepositVerifyPage.OpenUnverifyForm();
            unverifyForm.EnterRemarks(" This deposit is unverified.");
            unverifyForm.Submit();

            Assert.AreEqual("Offline deposit request has been unverified successfully", unverifyForm.ConfirmationMessage);
            Assert.AreEqual(referenceCode, unverifyForm.ReferenceCode);
            Assert.AreEqual("Unverified", unverifyForm.Status);
        }
    }
}
