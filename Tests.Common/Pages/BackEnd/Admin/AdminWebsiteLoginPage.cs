using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class AdminWebsiteLoginPage : BackendPageBase
    {
        public AdminWebsiteLoginPage(IWebDriver driver) : base(driver) {}

        public void ClearFields()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(d => _usernameField.Displayed);
            _usernameField.Clear();
            _passwordField.Clear();
        }

        public DashboardPage Login(string username, string password)
        {
            ClearFields();
            _usernameField.SendKeys(username);
            _passwordField.SendKeys(password);
            ClickLoginButton();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));

            //In R1.0 we will not have dashboard
            //var pageTitle = _driver.FindElementWait(By.XPath("//h1[text()='Dashboard Screen']"));
            var welcomeTitle = _driver.FindElementWait(By.XPath("//small[text()='Welcome,']"));
            wait.Until(d => welcomeTitle.Displayed);
            var page = new DashboardPage(_driver);
            page.Initialize();
            return page;
        }

        public void LoginWithIncompleteData(string username, string password)
        {
            _usernameField.SendKeys(username);
            _passwordField.SendKeys(password);
            ClickLoginButton();
        }

        public void LoginAsDeactivatedUser(string username, string password)
        {
            NavigateToAdminWebsite();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().Refresh();
            ClearFields();

            _usernameField.SendKeys(username);
            _passwordField.SendKeys(password);
            ClickLoginButton();
        }

        public void ClickLoginButton()
        {
            _loginButton.Click();
        }

        public string GetLoginErrorMsg()
        {
            return _usernamePasswordErrorMsg.Text;
        }

#pragma warning disable 649
        [FindsBy(How = How.Id, Using = "username")]
        private IWebElement _usernameField;

        [FindsBy(How = How.Id, Using = "password")]
        private IWebElement _passwordField;

        [FindsBy(How = How.XPath, Using = "//button[contains(., 'Login')]")]
        private IWebElement _loginButton;

        [FindsBy(How = How.XPath, Using = "//div[@id='errors']/ul/li[1]")]
        private IWebElement _usernamePasswordErrorMsg;

#pragma warning restore 649
    }
}