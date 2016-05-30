using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class RegisterPageStep3 : FrontendPageBase
    {
        #region Locators
        internal static readonly By SkipBonusLinkBy = By.XPath("//a[contains(@data-bind,'skipBonus')]");
        #endregion

        #region Elements
        public IWebElement SkipBonusLink
        {
            get { return _driver.FindElementWait(SkipBonusLinkBy); }
        }
        #endregion

     
        public FakePaymentServerPage SkipBonus()
        {   
            _driver.ScrollToElement(SkipBonusLink);
            _driver.WaitAndClickElement(SkipBonusLink);
          
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.PollingInterval = TimeSpan.FromMilliseconds(250);
            wait.Until(d => !_driver.Url.EndsWith("payment/issue"));
            _driver.WaitForJavaScript();

            var page = new FakePaymentServerPage(_driver);
            page.Initialize();
            return page;
        }

        public RegisterPageStep3(IWebDriver driver) : base(driver) { }
    }
}
