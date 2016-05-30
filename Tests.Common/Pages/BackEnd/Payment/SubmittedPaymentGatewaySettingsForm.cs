using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedPaymentGatewaySettingsForm : BackendPageBase
    {
        private string viewPath = "payments/payment-gateway-settings/details";
        public SubmittedPaymentGatewaySettingsForm(IWebDriver driver)
            : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public string AlertMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-danger']")); }
        }

        public string Licensee
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='"+viewPath+"']//div[@data-bind='with: form.fields.licensee']/p")
                            );
            }
        }

        public string Brand
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='" + viewPath + "']//div[@data-bind='with: form.fields.brand']/p")
                            );
            }
        }

        public string OnlinePaymentMethodName
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'fields.onlinePaymentMethodName')]")
                        );
            }
        }

        public string PaymentGatewayName
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='" + viewPath + "']//div[@data-bind='with: form.fields.paymentGatewayName']/p")
                            );
            }
        }

        public string Channel
        {
            get
            {
                return
                    _driver.FindElementValue(
                       By.XPath("//p[contains(@data-bind, 'fields.channel')]")
                            );
            }
        }

        public string EntryPoint
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'fields.entryPoint')]")
                            );
            }
        }


        public string Remarks
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.remarks')]")
                            );
            }
        }
    }
}