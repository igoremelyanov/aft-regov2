using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment
{
    public class EditBankForm : BackendPageBase
    {
        public EditBankForm(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedBankForm Submit(string licensee, string brand, string bankName)
        {
            SelectLicenseeBrand(
                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"),
                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"),
                licensee,
                By.XPath("//select[contains(@data-bind, 'options: Model.brands')]"),
                brand);

            var bankNameField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.bankName')]"));
            bankNameField.Clear();
            bankNameField.SendKeys(bankName);

            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.remarks')]"));
            remarksField.Clear();
            remarksField.SendKeys("new bank");

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();

            var form = new SubmittedBankForm(_driver);

            return form;
        }
    }
} 