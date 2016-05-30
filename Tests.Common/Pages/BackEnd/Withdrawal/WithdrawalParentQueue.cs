using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Withdrawal
{
    public class WithdrawalParentQueue : BackendPageBase
    {
        internal static readonly By ScrolGridDropdown = By.XPath("//select[contains(@class,'ui-pg-selbox')]");
        internal static readonly By WithdrawalStatusBy = By.XPath(".//td[contains(@aria-describedby,'Status')]");

        public IWebElement FindAndSelectWithdrawalRecord(string username, string amount)
        {
            new SelectElement(_driver.FindElementWait(ScrolGridDropdown)).SelectByText("100");
            _driver.WaitForJavaScript();

            var recordXPath = string.Format("//table[contains(@id, 'table_')]//tr[contains(., '{0}') and contains(., '{1}')]",
                username, amount);
            var recordInGrid = _driver.FindElementWait(By.XPath(recordXPath));
            recordInGrid.Click();
            return recordInGrid;
            //var element = _driver.FindElementSafely(By.XPath(recordXPath));
            //if (element != null) element.Click();
            //return element;
        }

        public string GetWithdrawalStatus(IWebElement record)
        {
            return record.FindElement(WithdrawalStatusBy).Text;
        }

        public WithdrawalParentQueue(IWebDriver driver)
            : base(driver)
        {
        }
    }
}
