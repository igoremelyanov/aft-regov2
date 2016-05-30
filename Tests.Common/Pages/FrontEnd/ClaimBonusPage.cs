using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using System;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class ClaimBonusPage : FrontendPageBase
    {

        #region Locators
        internal static readonly By ClaimButtonBy = By.XPath("//button[@data-i18n='common.validate']");
        internal static readonly By MessageBy = By.XPath("//a[@class='cashier']");
        #endregion

        #region Elements
        public IWebElement ClaimButton
        {
            get { return _driver.FindElementWait(ClaimButtonBy); }
        }
        public IWebElement MessageValue
        {
            get { return _driver.FindElementWait(MessageBy); }
        }

        #endregion


        public ClaimBonusPage(IWebDriver driver) : base(driver) { }
        

        public String Message()
        {
            return MessageValue.Text;
        }

        public void ClaimBonus()
        {
            ClaimButton.Click();
        }
    }
}