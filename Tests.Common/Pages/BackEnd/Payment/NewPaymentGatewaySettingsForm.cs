using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewPaymentGatewaySettingsForm : BackendPageBase
    {
        public NewPaymentGatewaySettingsForm(IWebDriver driver) : base(driver) { }

        public SubmittedPaymentGatewaySettingsForm Submit(PaymentGatewaySettingsData data)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'payment-gateway-settings-licensee')]"),
                By.XPath("//select[contains(@id, 'payment-gateway-settings-licensee')]"), data.Licensee, By.XPath("//select[contains(@id, 'payment-gateway-settings-brand')]"), data.Brand);

            _onlinePaymentMethodNameField.SendKeys(data.OnlinePaymentMethodName);

            var paymentGatewayName = new SelectElement(_paymentGatewayName);
            paymentGatewayName.SelectByText(data.PaymentGatewayName);

            _channel.Clear();
            _channel.SendKeys(data.Channel);

            _entryPoint.Clear();
            _entryPoint.SendKeys(data.EntryPoint);
            _remarks.Clear();
            _remarks.SendKeys(data.Remarks);
            _driver.ScrollPage(0, 600);
            _saveButton.Click();
            var submittedForm = new SubmittedPaymentGatewaySettingsForm(_driver);
            return submittedForm;
        }


        #pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.onlinePaymentMethodName')]")]
        private IWebElement _onlinePaymentMethodNameField;

        [FindsBy(How = How.XPath, Using = "//select[contains(@id, 'payment-gateway-settings-payment-gateway-name')]")]
        private IWebElement _paymentGatewayName;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.channel')]")]
        private IWebElement _channel;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.entryPoint')]")]
        private IWebElement _entryPoint;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@data-bind, 'value: fields.remarks')]")]
        private IWebElement _remarks;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/payment-gateway-settings/details']//button[text()='Save']")]
        private IWebElement _saveButton;
        #pragma warning restore 649


    }
}
