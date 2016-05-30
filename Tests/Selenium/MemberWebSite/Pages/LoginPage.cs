using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    class LoginPage : BasePage
    {
         public LoginPage(IWebDriver driver) : base(driver)
        {
        }
         public override String PageTitle()
         {
             return "Welcome to rego";
         }
#pragma warning disable 649
#pragma warning disable 169
        #region Locators

        //[FindsBy(How = How.XPath, Using = "//a[@href='#loginForm']")]
        //private IWebElement _loginButton;

        [FindsBy(How = How.CssSelector, Using = "input[placeholder='Username']")]
        private IWebElement _usernameField;

        [FindsBy(How = How.CssSelector, Using = "input[placeholder='Password']")]
        private IWebElement _passwordField;

        [FindsBy(How = How.XPath, Using = "//button[contains(., 'login')]")]
        private IWebElement _signInButton;

        [FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/Register']")]
        private IWebElement _registerButton;

        [FindsBy(How = How.XPath, Using = "//div[@id='login-messages']//li")]
        private IWebElement _errorMsg;
        #endregion
#pragma warning restore 649

        public MyAccountPage SubmitLogin(string username, string password)
        {
            EnterText(_usernameField, username);
            EnterText(_passwordField, password);
            SimpleClickElement(_signInButton);
            MyAccountPage myAccountPage = new MyAccountPage(_driver);
            return myAccountPage;
        }

        public string GetErrorMsg()
        {
            return _errorMsg.Text;
        }


        public RegistrationPage GoToRegisterPage()
        {
            SimpleClickElement(_registerButton);
            RegistrationPage registrationPage = new RegistrationPage(_driver);
            return registrationPage;   
        }
    }
}
