using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentSettingsPage : BackendPageBase
    {
        public PaymentSettingsPage(IWebDriver driver) : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "payment-settings-name-search", "payment-settings-search-button");
            }
        }

        public string GetStatus(string brand, string currency, string viplevel)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);
            var status = _driver.FindElementValue(By.XPath("//tr[@aria-selected='true']//td[contains(@aria-describedby, '_Enabled')]"));
            return status;
        }

        public NewPaymentSettingsForm OpenNewPaymentSettingsForm()
        {
            //TODO: remove header when defect: AFTREGO-3387 will be fixed
            WaitHelper.WaitUntil(() => _driver.FindElement(
    By.XPath("//*[contains(@id,'jqgrid-title-bar')]//button[@name='new']")).Displayed);
            var newButton = WaitHelper.WaitResult(() => _driver.FindElement(
                By.XPath("//*[contains(@id,'jqgrid-title-bar')]//button[@name='new']")));
            newButton.Click();
            var form = new NewPaymentSettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public PaymentSettingsActivateDeactivateDialog Activate(string brand, string currency, string viplevel, string remark)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);

            var activateButton =
                _driver.FindElementWait(By.XPath("//button[@name='acticate']"));
            activateButton.Click();
            var dialog = new PaymentSettingsActivateDeactivateDialog(_driver);
            dialog.EnterRemark(remark);
            dialog.Activate();
            Assert.AreEqual("The Payment Setting has been successfully activated", dialog.ConfirmationMessage);
            dialog.Close();
            return dialog;
        }

        public PaymentSettingsActivateDeactivateDialog Deactivate(string brand, string currency, string viplevel, string remark)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);

            var deactivateButton =
                _driver.FindElementWait(
                    By.XPath("//button[@name='deactivate']"));
            deactivateButton.Click();

            var dialog = new PaymentSettingsActivateDeactivateDialog(_driver);
            dialog.EnterRemark(remark);
            dialog.Deactivate();
            Assert.AreEqual("The Payment Setting has been successfully deactivated", dialog.ConfirmationMessage);
            dialog.Close();
            return dialog;
        }


        public EditPaymentSettingsForm OpenEditPaymentSettingsForm(string brand, string currency, string viplevel)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);
            var editButton =
                _driver.FindElementWait(
                    By.XPath(
                        "//div[@id='payment-settings-grid']//button[@name='edit']"));
            editButton.Click();

            var form = new EditPaymentSettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public void FindPaymentSettingsRecord(string brand, string currency, string viplevel)
        {
            Grid.FilterGrid(brand, By.XPath("//input[contains(@data-bind,'value: $root.search')]"), By.Id("search-button"));
            var recordXPath =
                string.Format(
                    "//table[contains(@id,'table_')]//tr[contains(., '{0}') and contains(., '{1}') and contains(., '{2}')]",
                    brand, currency, viplevel);
            var recordInGrid = _driver.FindElementWait(By.XPath(recordXPath));
            recordInGrid.Click();
        }
    }
}