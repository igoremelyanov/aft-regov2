
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    class HomePage : BasePage
    {
        public HomePage(IWebDriver driver) : base(driver)
        {
        }

        public override String PageTitle()
        {
            return  "Welcome to rego";
        }

        #region Locators
#pragma warning disable 649

        [FindsBy(How = How.XPath, Using = "//a[contains(., 'login')]")]
        private IWebElement _signInButton;

        [FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/Register']")]
        private IWebElement _registerButton;
#pragma warning restore 649
        #endregion

        public void GoToRegistrationPage()
        {
            WaitForElementToBeClickable(_signInButton);
            SimpleClickElement(_registerButton);
        }

        public LoginPage GoToLoginForm()
        {
            WaitForElementToBeClickable(_signInButton);
            SimpleClickElement(_signInButton);
            LoginPage loginPage = new LoginPage(_driver);
            return loginPage;
        }
    }
}
