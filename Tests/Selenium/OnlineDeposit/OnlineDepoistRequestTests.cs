using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Svitlana: 04/25/2016 Ignored. Need refactoring  and update due to the lastest changes")]
    class OnlineDepoistRequestTests : SeleniumBaseForMemberWebsite
    {
        private RegistrationDataForMemberWebsite _player;
        private PlayerProfilePage _playerProfilePage;
        private ICommonSettingsProvider _settingsProvider;

        public override void BeforeAll()
        {
            base.BeforeAll();

            _settingsProvider = _container.Resolve<ICommonSettingsProvider>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();

            //create a player
            _player = playerTestHelper.CreatePlayerForMemberWebsite(currencyCode:"CAD", password:"123456");
            
        }

        public override void BeforeEach()
        {
            base.BeforeEach();

           _playerProfilePage = _driver.LoginToMemberWebsite(_player.Username, _player.Password);
        }
       
        [TestCase("500")]
        public void Can_submit_online_deposit_and_return_to_brand_via_member_site(string amount)
        {
            CashierPage _cashierPage = _playerProfilePage.Menu.OpenCashierPage();
            DepositOnlinePage _depositOnlinePage = _cashierPage.OpenOnlineDepositPage();
            _depositOnlinePage.EnterDepositAmount(amount);
            _depositOnlinePage.SubmitOnlineDeposit();
            var _fakePaymentServerPage = new FakePaymentServerPage(_driver);
            //Verify the deposit amount is correct
            Assert.AreEqual(amount, _fakePaymentServerPage.GetAmountValue());
            _fakePaymentServerPage.NotifyAndRedirect();

            Assert.IsTrue(_depositOnlinePage.GetDepositConfirmedValue().Contains(amount));

            Assert.AreEqual(amount, _depositOnlinePage.GetBalanceAmount());

        }
      
        [Test]
        public void Can_submit_online_deposit_and_receive_paynotify_via_member_site()
        {
            var balanceInfoPage = _playerProfilePage.Menu.ClickBalanceInformationMenu();
            var onlineDepositRequestPage = balanceInfoPage.Menu.ClickOnlineDepositSubmenu();

            var fakePaymentServerPage = onlineDepositRequestPage.Submit(amount: "200");
            var paymentProxyUrl = _settingsProvider.GetPaymentProxyUrl() + "payment/issue";
            Assert.AreEqual(paymentProxyUrl, fakePaymentServerPage.DepositWindowUrl);

            fakePaymentServerPage.SubmitNotify();
            Assert.AreEqual("SUCCESS", fakePaymentServerPage.AlertMessage);
            fakePaymentServerPage.BackToMemberSite();
        }
    
        [Test]
        public void Can_verify_and_approve_online_deposit()
        {
            var referenceCode = MakeOnlineDeposit("200", false);

            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerDepositVerifyPage = dashboardPage.Menu.ClickPlayerDepositVerifyItem();
            playerDepositVerifyPage.FilterGrid(_player.Username);
            playerDepositVerifyPage.SelectConfirmedDeposit(referenceCode);
            var form = playerDepositVerifyPage.OpenVerifyOnlineDepositForm();
            form.EnterRemarks("This deposit is verified.");
            form.Submit();

            Assert.AreEqual("Deposit request has been verified successfully", form.ConfirmationMessage);
            Assert.AreEqual(_player.Username, form.Username);
            Assert.AreEqual("Verified", form.Status);

            var playerDepositApprovePage = dashboardPage.Menu.ClickPlayerDepositApproveItem();
            playerDepositApprovePage.FilterGrid(_player.Username);
            playerDepositApprovePage.SelectVerifiedDeposit(referenceCode);
            var approveform = playerDepositApprovePage.OpenApproveOnlineDepositForm();
            approveform.EnterRemarks("This deposit is verified.");
            approveform.Submit();

            Assert.AreEqual("Deposit request has been approved successfully", approveform.ConfirmationMessage);
            Assert.AreEqual(_player.Username, form.Username);
            Assert.AreEqual("Approved", form.Status);
        }
        
        [Test]
        public void Can_verify_and_reject_online_deposit()
        {
            var referenceCode = MakeOnlineDeposit("200", false);

            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerDepositVerifyPage = dashboardPage.Menu.ClickPlayerDepositVerifyItem();
            playerDepositVerifyPage.FilterGrid(_player.Username);
            playerDepositVerifyPage.SelectConfirmedDeposit(referenceCode);
            var form = playerDepositVerifyPage.OpenVerifyOnlineDepositForm();
            form.EnterRemarks("This deposit is verified.");
            form.Submit();

            Assert.AreEqual("Deposit request has been verified successfully", form.ConfirmationMessage);
            Assert.AreEqual(_player.Username, form.Username);
            Assert.AreEqual("Verified", form.Status);

            var playerDepositApprovePage = dashboardPage.Menu.ClickPlayerDepositApproveItem();
            playerDepositApprovePage.FilterGrid(_player.Username);
            playerDepositApprovePage.SelectVerifiedDeposit(referenceCode);
            var approveform = playerDepositApprovePage.OpenRejectOnlineDepositForm();
            approveform.EnterRemarks("This deposit is rejected.");
            approveform.Submit();

            Assert.AreEqual("Deposit request has been rejected", approveform.ConfirmationMessage);
            Assert.AreEqual(_player.Username, form.Username);
            Assert.AreEqual("Rejected", form.Status);
        }
       
        [Test]
        public void Can_reject_online_deposit()
        {
            var referenceCode = MakeOnlineDeposit("200", false);

            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerDepositVerifyPage = dashboardPage.Menu.ClickPlayerDepositVerifyItem();
            playerDepositVerifyPage.FilterGrid(_player.Username);
            playerDepositVerifyPage.SelectConfirmedDeposit(referenceCode);
            var rejectForm = playerDepositVerifyPage.OpenRejectOnlineDepositForm();
            rejectForm.EnterRemarks("This deposit is rejected.");
            rejectForm.Submit();

            Assert.AreEqual("Deposit request has been rejected", rejectForm.ConfirmationMessage);
            Assert.AreEqual(_player.Username, rejectForm.Username);
            Assert.AreEqual("Rejected", rejectForm.Status);
        }

        private string MakeOnlineDeposit(string amount = "200", bool doNotify = true)
        {
            var balanceInfoPage = _playerProfilePage.Menu.ClickBalanceInformationMenu();
            var onlineDepositRequestPage = balanceInfoPage.Menu.ClickOnlineDepositSubmenu();
            var fakePaymentServerPage = onlineDepositRequestPage.Submit(amount: amount);
            

            if (doNotify)
                fakePaymentServerPage.SubmitNotify();
            else
                fakePaymentServerPage.Cancel();
            fakePaymentServerPage.BackToMemberSite();

            CashierPage _cashierPage = _playerProfilePage.Menu.OpenCashierPage();
            DepositOnlinePage _depositOnlinePage = _cashierPage.OpenOnlineDepositPage();
            _depositOnlinePage.EnterDepositAmount(amount);
            _depositOnlinePage.SubmitOnlineDeposit();
            //Verify the deposit amount is correct
            Assert.AreEqual(amount, fakePaymentServerPage.GetAmountValue());
            var referenceCode = fakePaymentServerPage.OrderId;
            fakePaymentServerPage.NotifyAndRedirect();


            return referenceCode;
        }
    }
}