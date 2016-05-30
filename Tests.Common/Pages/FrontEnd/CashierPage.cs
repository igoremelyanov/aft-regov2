using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class CashierPage : FrontendPageBase
    {
       
        #region Locators
        internal static readonly By OnlineDepositLinkBy = By.XPath("//a[contains(@data-i18,'balanceInformation.onlineDeposit')]");
        internal static readonly By OfflineDepositLinkBy = By.XPath("//a[contains(@data-i18,'balanceInformation.offlineDeposit')]");
        internal static readonly By BonusAmountBy = By.XPath("//*[@data-balance-type='bonusFormatted']");

        #endregion

        #region Elements
        public IWebElement OnlineDepositLink
        {
            get { return _driver.FindElementWait(OnlineDepositLinkBy); }
        }

        public IWebElement OfflineDepositLink
        {
            get { return _driver.FindElementWait(OfflineDepositLinkBy); }
        }

        public IWebElement BonusAmount
        {
            get { return _driver.FindElementWait(BonusAmountBy); }
        }

        #endregion

        public DepositOnlinePage OpenOnlineDepositPage()
        {
            _driver.WaitForElementClickable(OnlineDepositLink);
            OnlineDepositLink.Click();
            _driver.WaitForJavaScript();
            var page = new DepositOnlinePage(_driver);
            page.Initialize();
            return page;
        }

        public OfflineDepositRequestPage OpenOfflineDepositPage()
        {
            _driver.WaitForElementClickable(OfflineDepositLink);
            OfflineDepositLink.Click();
            _driver.WaitForJavaScript();
            var page = new OfflineDepositRequestPage(_driver);
            page.Initialize();
            return page;
        }

        public string GetBonusBalance()
        {
            return BonusAmount.Text;
        }

       public CashierPage (IWebDriver driver) : base(driver) { }
}
}
