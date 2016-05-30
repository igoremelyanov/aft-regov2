using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class OnlineDepositRequestPage : FrontendPageBase
    {
        public OnlineDepositRequestPage(IWebDriver driver) : base(driver) {}
   
        public FakePaymentServerPage Submit(string amount)
        {
            var amountField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: onlineAmount, numeric: onlineAmount']"));
            amountField.Clear();
            amountField.SendKeys(amount);

            var submitButton = _driver.FindElementWait(By.XPath("//button[@data-bind='click: submitOnlineDeposit ,enable: !onlineDepositRequestInProgress()']"));
            submitButton.Click();
            return new FakePaymentServerPage(_driver);
        }

    }
}