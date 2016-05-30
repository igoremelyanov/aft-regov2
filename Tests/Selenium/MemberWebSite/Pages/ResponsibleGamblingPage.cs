using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    class ResponsibleGamblingPage : BasePage
    {
        public ResponsibleGamblingPage(IWebDriver driver)
            : base(driver)
        {
        }

        public override String PageTitle()
        {
            return "My account - Rego";
        }

        #region Locators
#pragma warning disable 649

        [FindsBy(How = How.Id, Using = "time-out")]
        private IWebElement _timeOutRadioButton;

        [FindsBy(How = How.XPath, Using = "//button[contains(., 'Save')]")]
        private IWebElement _saveButton;
#pragma warning restore 649
#endregion

        public void SetTimeOut()
        {
            SimpleClickElement(_timeOutRadioButton);
            SimpleClickElement(_saveButton);
        }
   

    }
}
