using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditPaymentGatewaySettingsForm : BackendPageBase
    {
        public EditPaymentGatewaySettingsForm(IWebDriver driver) : base(driver) { }

        public SubmittedPaymentGatewaySettingsForm Submit(PaymentGatewaySettingsData data)
        {
            _clearButton.Click();
            //_onlinePaymentMethodNameField.SendKeys(data.OnlinePaymentMethodName);

            //var paymentGatewayName = new SelectElement(_paymentGatewayName);
            //paymentGatewayName.SelectByText(data.PaymentGatewayName);

            //_channel.Clear();
            //_channel.SendKeys(data.Channel);

            _entryPoint.Clear();
            _entryPoint.SendKeys(data.EntryPoint);
            _entryPoint.SendKeys(Keys.Enter);

            _remarks.Clear();
            _remarks.SendKeys(data.Remarks);
            _remarks.SendKeys(Keys.Enter);

            _driver.ScrollPage(0, 600);

            _saveButton.Click();
            var submittedForm = new SubmittedPaymentGatewaySettingsForm(_driver);
            return submittedForm;
        }      

#pragma warning disable 649
        //[FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.onlinePaymentMethodName')]")]
        //private IWebElement _onlinePaymentMethodNameField;

        //[FindsBy(How = How.XPath, Using = "//select[contains(@id, 'payment-gateway-settings-payment-gateway-name')]")]
        //private IWebElement _paymentGatewayName;

        //[FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.channel')]")]
        //private IWebElement _channel;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.entryPoint')]")]
        private IWebElement _entryPoint;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@data-bind, 'value: fields.remarks')]")]
        private IWebElement _remarks;        

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/payment-gateway-settings/details']//button[text()='Save']")]
        private IWebElement _saveButton;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/payment-gateway-settings/details']//button[text()='Clear']")]
        private IWebElement _clearButton;
#pragma warning restore 649
    }
}