using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentGatewaySettingsActivateDialog : BackendPageBase
    {
        public PaymentGatewaySettingsActivateDialog(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'text: message()')]")); }
        }

        public void EnterRemark(string remark)
        {
            var remarks = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: remarks')]"));
            remarks.SendKeys(remark);
        }

        public void Deactivate()
        {
            var btn = _driver.FindElementWait(By.XPath("//div[@data-view='payments/payment-gateway-settings/status-dialog']//button[text()='Deactivate']"));
            btn.Click();
        }

        public void Activate()
        {
            var btn = _driver.FindElementWait(By.XPath("//div[@data-view='payments/payment-gateway-settings/status-dialog']//button[text()='Activate']"));
            btn.Click();
        }

        public PaymentGatewaySettingsPage Close()
        {
            var closeButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/payment-gateway-settings/status-dialog']//button[text()='Close']"));
            closeButton.Click();
            var page = new PaymentGatewaySettingsPage(_driver);
            return page;
        }
    }
}