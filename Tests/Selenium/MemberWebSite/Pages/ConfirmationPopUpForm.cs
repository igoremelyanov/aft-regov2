using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    class ConfirmationPopUpForm : BasePage
    {
        public ConfirmationPopUpForm(IWebDriver driver) : base(driver) { }


        public override String PageTitle()
        {
            return  "My account - Rego";
        }
#region Locators
#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//button[contains(., 'yes')]")]
        private IWebElement _yesButton;
        //[FindsBy(How = How.XPath, Using = "//button[contains(., 'no')]")]
        //private IWebElement _noButton;
#pragma warning restore 649
#endregion
        public void ApproveTimeOut()
        {
            WaitForAjax();
            WaitForElementToBeClickable(_yesButton);
            SimpleClickElement(_yesButton);
        }
    }
}
