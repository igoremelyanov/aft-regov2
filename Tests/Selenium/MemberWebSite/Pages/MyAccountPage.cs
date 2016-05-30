using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    class MyAccountPage : BasePage
    {
        public MyAccountPage(IWebDriver driver)
            : base(driver)
        {
        }

        public override String PageTitle()
        {
            return "My Account - Rego";
        }
#pragma warning disable 649
        #region Locators

        [FindsBy(How = How.XPath, Using = "//a[contains(., 'responsible gambling')]")]
        private IWebElement _responsibleGamblingButton;

        #endregion
#pragma warning restore 649
        public ResponsibleGamblingPage NavigateToResponsibleGamblingPage()
        {
            SimpleClickElement(_responsibleGamblingButton);
            ResponsibleGamblingPage responsibleGamblingPage = new ResponsibleGamblingPage(_driver);
            return responsibleGamblingPage;
        
        }
        
    }
}
