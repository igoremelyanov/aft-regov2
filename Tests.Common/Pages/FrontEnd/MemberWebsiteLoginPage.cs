using System;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class MemberWebsiteLoginPage : FrontendPageBase
    {
        public MemberWebsiteLoginPage(IWebDriver driver) : base(driver) {}

        protected override string GetPageUrl()
        {
            return "en-US/Home/Login";
        }

        public PlayerProfilePage Login(string username, string password)
        {
            Initialize();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            _loginButton.Click();
            wait.Until(d => _usernameField.Displayed);
            _usernameField.Clear();
            _usernameField.SendKeysWithOnChangeEvent(username);
            
            _passwordField.Clear();
            _passwordField.SendKeysWithOnChangeEvent(password);
            ClickSignInButton();
            var playerOverviewPage = new PlayerOverviewPage(_driver);
            playerOverviewPage.Initialize();
            var playerProfilePage = playerOverviewPage.HeaderMenu.OpenMyAccount();
            playerProfilePage.Initialize();
            return playerProfilePage;
        }

        public void TryToLogin(string username, string password)
        {
            _driver.WaitForJavaScript();
            Initialize();
            _loginButton.Click();

            _usernameField.Clear();
            _usernameField.SendKeys(username);

            _passwordField.Clear();            
            _passwordField.SendKeys(password);

            ClickSignInButton();
            Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for message box fade in animation
        }

        public void ClickSignInButton()
        {
            _signInButton.Click();
            _driver.WaitForJavaScript();
        }

        public string GetErrorMsg()
        {
            return _errorMsg.Text;
        }

#pragma warning disable 649
        #region Main Page Locators

        [FindsBy(How = How.XPath, Using = "//a[@href='#loginForm']")]
        private IWebElement _loginButton;

        #endregion


        [FindsBy(How = How.XPath, Using = "//*[@data-bind='value: username']")]
        private IWebElement _usernameField;

        [FindsBy(How = How.XPath, Using = "//*[@data-bind='value: password']")]
        private IWebElement _passwordField;

        [FindsBy(How = How.XPath, Using = "//button[contains(., 'login')]")]
        private IWebElement _signInButton;

        [FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/Register']")]
#pragma warning disable 169
        private IWebElement _registerButton;
#pragma warning restore 169

        [FindsBy(How = How.XPath, Using = "//div[@class='form-group message has-error']//p[@class='msg']")]
        private IWebElement _errorMsg;
#pragma warning restore 649

        public RegisterPage GoToRegisterPage()
        {
            var registerPage = new RegisterPage(_driver);
            registerPage.NavigateToMemberWebsite();
            return registerPage;
        }
    }
}