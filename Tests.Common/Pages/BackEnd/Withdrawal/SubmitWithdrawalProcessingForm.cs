using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Withdrawal
{  

    //Reresents parent form for manual  withdrawal processing
    public class SubmitWithdrawalProcessingForm : BackendPageBase
    {
        internal static readonly By SuccessAlertBy = By.XPath("//div[contains(@class,'alert-success')]");
        internal static readonly By SubmitButtonBy = By.XPath("//button[contains(@data-bind,'submit')]");
        internal static readonly By InputRemarkBy = By.XPath("//textarea[contains(@data-bind,'remarks')]");

        public IWebElement SuccessAlert
        {
            get { return _driver.FindElementWait(SuccessAlertBy); }
        }

        public IWebElement SubmitButton
        {
            get { return _driver.FindElementWait(SubmitButtonBy); }
        }

        public IWebElement InputRemark
        {
            get { return _driver.FindElementWait(InputRemarkBy); }
        }

        public void SubmitProcessing(string remark)
        {
            InputRemark.Clear();
            InputRemark.SendKeys(remark);
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(SubmitButton);
        }

        public SubmitWithdrawalProcessingForm(IWebDriver driver)
            : base(driver)
        {
        }
    }
}
