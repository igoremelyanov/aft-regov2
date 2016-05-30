using System;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.OnlineDeposit 
{
   [Ignore ("Svitlana: 27/04/2016. Needs to be updated due to changes in UI, additional popup appears")]
    public class OnlineDepositCashierTests : SeleniumBaseForMemberWebsite
    {
        private MemberWebsiteLoginPage _brandWebsiteLoginPage;
        private RegistrationDataForMemberWebsite _playerData;
        private PlayerProfilePage _playerProfilePage;
        private CashierPage _cashierPage;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _playerData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("RMB");

            // register a player on a brand website
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
            var brandWebsiteRegisterPage = _brandWebsiteLoginPage.GoToRegisterPage();
            var _registerPageStep2 = brandWebsiteRegisterPage.Register(_playerData);

            //verify the player was registered
            Assert.AreEqual("NEXT STEP: DEPOSIT BELOW", _registerPageStep2.GetSuccessMessage());

            _brandWebsiteLoginPage.NavigateToMemberWebsite();
             _playerProfilePage = _brandWebsiteLoginPage.Login(_playerData.Username, _playerData.Password);
             _cashierPage = _playerProfilePage.Menu.OpenCashierPage();
        }

        [TestCase("1300")]
        public void Can_create_online_from_cashier_page_by_entering_the_amount(string amount)
        {
            var _onlineDepositPage = _cashierPage.OpenOnlineDepositPage();

            //enter deposit amount manually
            _onlineDepositPage.EnterDepositAmount(amount);

            //submit deposit request
            _onlineDepositPage.SubmitOnlineDeposit();

            var _fakePaymentServerPage = new FakePaymentServerPage(_driver);

            //Verify the deposit amount is correct
            Assert.AreEqual(amount, _fakePaymentServerPage.GetAmountValue());

            //Notify and Redirect
            _fakePaymentServerPage.NotifyAndRedirect();
            
            Assert.IsTrue(_onlineDepositPage.GetDepositConfirmedValue().Contains(amount));
            // fix amount formatting for assert
            Assert.AreEqual(amount, _onlineDepositPage.GetBalanceAmount().Replace(",", String.Empty));

        }

        [TestCase("1,250")]
        [TestCase("2,500")]
        [TestCase("3,750")]
        [TestCase("5,000")]
        public void Can_create_online_from_cashier_page_by_entering_the_suggested_amount(string amount)
        {
            var _onlineDepositPage = _cashierPage.OpenOnlineDepositPage();

            //select the suggested deposit amount by clicking the button
            _onlineDepositPage.SelectDepositAmount(amount);

            //submit deposit request
            _onlineDepositPage.SubmitOnlineDeposit();

            var _fakePaymentServerPage = new FakePaymentServerPage(_driver);

            //Verify the deposit amount is correct
            Assert.AreEqual(amount.Replace(",", String.Empty), _fakePaymentServerPage.GetAmountValue());

            //Notify and Redirect
            _fakePaymentServerPage.NotifyAndRedirect();

            Assert.IsTrue(_onlineDepositPage.GetDepositConfirmedValue().Contains(amount.Replace(",", String.Empty)));

            Assert.AreEqual(amount, _onlineDepositPage.GetBalanceAmount());

        }
    }
}
