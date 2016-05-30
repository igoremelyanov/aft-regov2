using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.MemberWebsite
{
    //[Ignore("Until Pavel's fixes for Cashier page OnlineDeposit on Member site AFTREGO-3705 - 12-Feb-2016, Igor")]
    class CashierTests : SeleniumBaseForMemberWebsite
    {
        private MemberWebsiteLoginPage _brandWebsiteLoginPage;
        private PlayerTestHelper _playerTestHelper;
        private PaymentTestHelper _paymentTestHelper;
        private RegistrationDataForMemberWebsite _player;
        private PlayerProfilePage _playerProfilePage;
   //     private BalanceDetailsPage _balanceDetailsPage;
        private CashierPage _cashierPage;
 //       private OfflineDepositRequestPage _offlineDepositPge;
        private const decimal DepositAmount = 200;

        public override void BeforeAll()
        {
            base.BeforeAll();
            //create a player
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");

            //deposit money to the player's main balance
            _paymentTestHelper.MakeDeposit(_player.Username, DepositAmount);
            
            //navigate to brand website
            _brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            //log in to player's profile
            _playerProfilePage = _brandWebsiteLoginPage.Login(_player.Username, _player.Password);
            _cashierPage = _playerProfilePage.Menu.OpenCashierPage();
        }
        
        [Test]
        [Ignore("Failing with 500 on RC-1.0 - AFTREGO-4661 - 25-April-2016, Igor")]
        public void Can_submit_offline_deposit_request_on_member_website()
        {
            var _cashierPage = _playerProfilePage.Menu.OpenCashierPage();
            var _offlineDepositPage = _cashierPage.OpenOfflineDepositPage();
            _offlineDepositPage.Submit(amount:"101", playerRemark:"my deposit");

            Assert.AreEqual("Congratulation on your deposit!", _offlineDepositPage.GetConfirmationMessage());
        }

        //[Test]
        //public void Can_fund_in_fund_out_amount_on_member_website()
        //{
        //    const decimal amount =  DepositAmount / 4;
        //    var transferFundRequestPage = _balanceDetailsPage.Menu.ClickTransferFundSubMenu();
        //    transferFundRequestPage.FundIn(amount);

        //    Assert.That(transferFundRequestPage.ConfirmationMessage, Is.StringContaining("Transfer fund request sent successfully."));
        //    Assert.That(transferFundRequestPage.ConfirmationMessage, Is.StringContaining("Transfer ID:"));
        //    var productWalletAmount = string.Format(amount + ".00");
        //    Assert.AreEqual(productWalletAmount, transferFundRequestPage.Balance);

        //    transferFundRequestPage.FundOut(amount);
        //    Assert.That(transferFundRequestPage.ConfirmationMessage, Is.StringContaining("Transfer fund request sent successfully."));
        //    Assert.AreEqual("0.00", transferFundRequestPage.Balance);
        //}


    }
}
