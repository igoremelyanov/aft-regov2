using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentGatewaySettingsPage : BackendPageBase
    {
        public PaymentGatewaySettingsPage(IWebDriver driver)
            : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "payment-gateway-settings-name-search", "payment-gateway-settings-search-button");
            }
        }


        public string GetStatus(string brand, string onlinePaymentMethodName)
        {
            FindPaymentSettingsRecord(brand, onlinePaymentMethodName);
            var status = _driver.FindElementValue(By.XPath("//tr[@aria-selected='true']//td[contains(@aria-describedby, '_Status')]"));
            return status;
        }

        public NewPaymentGatewaySettingsForm OpenNewPaymentGatewaySettingsForm()
        {
            WaitHelper.WaitUntil(
                () => _driver.FindElement(
                    By.XPath("//*[contains(@id,'jqgrid-title-bar')]//button[@name='new']")).Displayed
                );

            var newButton = WaitHelper.WaitResult(
                () => _driver.FindElement(
                    By.XPath("//*[contains(@id,'jqgrid-title-bar')]//button[@name='new']"))
                );
            newButton.Click();
            var form = new NewPaymentGatewaySettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public EditPaymentGatewaySettingsForm OpenEditPaymentGatewaySettingsForm(string brand, string onlinePaymentMethodName)
        {
            FindPaymentSettingsRecord(brand, onlinePaymentMethodName);
            var editButton =
                _driver.FindElementWait(
                    By.XPath(
                        "//div[@id='payment-gateway-settings-grid']//button[@name='edit']"));
            editButton.Click();

            var form = new EditPaymentGatewaySettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public void FindPaymentSettingsRecord(string brand, string onlinePaymentMethodName)
        {
            Grid.FilterGrid(brand, By.XPath("//input[contains(@data-bind,'value: $root.search')]"), By.Id("search-button"));

            var recordXPath =
                string.Format(
                    "//table[contains(@id,'table_')]//tr[contains(., '{0}') and contains(., '{1}')]",
                    brand, onlinePaymentMethodName);

            var recordInGrid = _driver.FindElementWait(By.XPath(recordXPath));
            recordInGrid.Click();
        }

        public PaymentGatewaySettingsActivateDialog Activate(string brand, string onlinePaymentMethodName)
        {
            FindPaymentSettingsRecord(brand, onlinePaymentMethodName);

            var activateButton =
                 _driver.FindElementWait(
                    By.XPath("//button[@name='activate']"));
            activateButton.Click();
            var dialog = new PaymentGatewaySettingsActivateDialog(_driver);
            dialog.EnterRemark("activate remark");
            dialog.Activate();
            dialog.Close();
            return dialog;
        }

        public PaymentGatewaySettingsActivateDialog Deactivate(string brand, string onlinePaymentMethodName)
        {
            FindPaymentSettingsRecord(brand, onlinePaymentMethodName);

            var deactivateButton =
                _driver.FindElementWait(
                    By.XPath("//button[@name='deactivate']"));
            deactivateButton.Click();

            var dialog = new PaymentGatewaySettingsActivateDialog(_driver);
            dialog.EnterRemark("deactivate remark");
            dialog.Deactivate();
            dialog.Close();
            return dialog;
        }
    }
}