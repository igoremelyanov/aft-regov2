using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment
{
    public class NewBankForm : BackendPageBase
    {
        public NewBankForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath = "//li[contains(@class, 'active') and not(contains(@class, 'inactive'))]";

        public string Title
        {
            get { return _driver.FindElementWait(By.XPath(FormXPath + "//span[text()='New Bank']")).Text; }
        }

        private void SelectCountry(string countryName)
        {
            var countryBy = By.XPath("//select[contains(@data-bind, 'options: Model.countries')]");

            if (!_driver.FindElementsWait(countryBy).Any(x => x.Displayed && x.Enabled))
                return;

            var countryList = _driver.FindElementWait(countryBy);
            var countryField = new SelectElement(countryList);
            countryField.SelectByText(countryName);
        }

        public SubmittedBankForm Submit(string brand, string bankId, string bankName, string countryName, string remarks)
        {
            SelectLicenseeBrand(
                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"),
                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"), 
                "Flycow",
                By.XPath("//select[contains(@data-bind, 'options: Model.brands')]"), 
                brand);

            var bankIdField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.bankId')]"));
            bankIdField.SendKeys(bankId);

            var bankNameField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.bankName')]"));
            bankNameField.SendKeys(bankName);

            SelectCountry(countryName);

            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.remarks')]"));
            remarksField.SendKeys("new bank");

            ClickSaveButton();

            var form = new SubmittedBankForm(_driver);

            return form;
        }

        public SubmittedBankForm SubmitWithLicensee(string licensee, string brand, string bankId, string bankName, string countryName, string remarks)
        {
            SelectLicenseeBrand(
                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"),
                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"),
                licensee,
                By.XPath("//select[contains(@data-bind, 'options: Model.brands')]"),
                brand);

            var bankIdField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.bankId')]"));
            bankIdField.SendKeys(bankId);

            var bankNameField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.bankName')]"));
            bankNameField.SendKeys(bankName);

            SelectCountry(countryName);

            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.remarks')]"));
            remarksField.SendKeys("new bank");

            ClickSaveButton();

            var form = new SubmittedBankForm(_driver);

            return form;
        }

        public void ClickSaveButton()
        {
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
        }
    }
}