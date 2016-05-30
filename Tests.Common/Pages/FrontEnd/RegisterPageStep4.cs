using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class RegisterPageStep4 : FrontendPageBase
    {
        #region Locators
        internal static readonly By SuccessMessageBy = By.XPath("//h2[contains(@class,'middlewhite')]");
        internal static readonly By BalanceAmountBy = By.XPath("//span[contains(@data-balance-type,'playable')]");
        #endregion

        #region Elements
        public IWebElement SuccessMessage
        {
            get { return _driver.FindElementWait(SuccessMessageBy); }
        }

        public IWebElement BalanceAmount
        {
            get { return _driver.FindElementWait(BalanceAmountBy); }
        }
        #endregion

        public string GetSuccessMessage()
        {
            return SuccessMessage.Text;
        }

        public string GetBalanceAmount()
        {
            return BalanceAmount.Text;
        }

        public RegisterPageStep4(IWebDriver driver) : base(driver) { }
    }
}
