using System;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.OnlineDeposit
{
   public class OnlineDepositRegistrationTests : SeleniumBaseForMemberWebsite
    {
        private MemberWebsiteLoginPage _brandWebsiteLoginPage;
        private RegistrationDataForMemberWebsite _playerData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _playerData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("RMB");

        }

        [TestCase("555.00", "555")]
        public void Can_create_online_deposit_during_registration_by_entering_the_amount(string longAmount, string shortAmount)
        {
            // register a player on a brand website
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
            var brandWebsiteRegisterPage = _brandWebsiteLoginPage.GoToRegisterPage();
            var registerPageStep2 = brandWebsiteRegisterPage.Register(_playerData);

            //verify the player was registered
            Assert.AreEqual("NEXT STEP: DEPOSIT BELOW", registerPageStep2.GetSuccessMessage());

            //verify the user was redirected to the registration step2
            Assert.True(_driver.IfUrlContainsSubstr("RegisterStep2"));

            //enter deposit amount manually
            registerPageStep2.EnterDepositAmount(longAmount);

            //submit deposit request
            registerPageStep2.SubmitOnlineDeposit();

            var fakePaymentServerPage = new FakePaymentServerPage(_driver);

            //Verify the deposit amount is correct
            var fakePaymentServerPageAmount = fakePaymentServerPage.GetAmountValue();
            Assert.AreEqual(shortAmount, fakePaymentServerPageAmount);

            //Notify and Redirect
            fakePaymentServerPage.NotifyAndRedirect();

            var registerPageStep4 = new RegisterPageStep4(_driver);

            //verify the deposit  was submited correct
            Assert.AreEqual("CONGRATULATION ON YOUR DEPOSIT!", registerPageStep4.GetSuccessMessage());

            var step4BalanceAmount = registerPageStep4.GetBalanceAmount();
            Assert.AreEqual(longAmount, step4BalanceAmount);
        }

        [TestCase("-100") ]
        public void Can_not_create_online_deposit_during_registration_by_entering_wrong_amount(string amount)
        {
            // register a player on a brand website
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
            var brandWebsiteRegisterPage = _brandWebsiteLoginPage.GoToRegisterPage();
            var registerPageStep2 = brandWebsiteRegisterPage.Register(_playerData);

            //verify the player was registered
            Assert.AreEqual("NEXT STEP: DEPOSIT BELOW", registerPageStep2.GetSuccessMessage());

            //verify the user was redirected to the registration step2
            Assert.True(_driver.IfUrlContainsSubstr("RegisterStep2"));

            //enter deposit amount manually
            registerPageStep2.EnterDepositAmount(amount);

            //submit deposit request
            registerPageStep2.SubmitOnlineDeposit();

            Assert.AreEqual("Amount must be greater than zero.", registerPageStep2.GetErrorMessage());
        }

        [TestCase("1250")]
        [TestCase("2500")]
        [TestCase("3750")]
        [TestCase("5000")]
        public void Can_create_online_deposit_during_registration_by_selecting_the_suggested_amount(string stringAmount)
        {
            var decimalAmount = decimal.Parse(stringAmount);

            // register a player on a brand website
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
            var brandWebsiteRegisterPage = _brandWebsiteLoginPage.GoToRegisterPage();
            var registerPageStep2 = brandWebsiteRegisterPage.Register(_playerData);

            //verify the player was registered
            Assert.AreEqual("NEXT STEP: DEPOSIT BELOW", registerPageStep2.GetSuccessMessage());

            //verify the user was redirected to the registration step2
            Assert.True(_driver.IfUrlContainsSubstr("RegisterStep2"));

            //select the suggested deposit amount by clicking the button
            var quickSelectAmount = decimalAmount.Format("RMB", false, DecimalDisplay.ShowNonZeroOnly);
            registerPageStep2.SelectDepositAmount(quickSelectAmount);

            //submit deposit request
            registerPageStep2.SubmitOnlineDeposit();

            var fakePaymentServerPage = new FakePaymentServerPage(_driver);

            //Verify the deposit amount is correct
            var fakePaymentServerPageAmount = fakePaymentServerPage.GetAmountValue();
            Assert.AreEqual(stringAmount, fakePaymentServerPageAmount);

            //Notify and Redirect
            fakePaymentServerPage.NotifyAndRedirect();

            var registerPageStep4 = new RegisterPageStep4(_driver);

            //verify the deposit  was submited correct
            Assert.AreEqual("CONGRATULATION ON YOUR DEPOSIT!", registerPageStep4.GetSuccessMessage());

            var step4BalanceAmount = registerPageStep4.GetBalanceAmount();
            var amountFormatted = decimalAmount.Format("RMB", false, DecimalDisplay.AlwaysShow);
            Assert.AreEqual(amountFormatted, step4BalanceAmount);
        }
    }
}


