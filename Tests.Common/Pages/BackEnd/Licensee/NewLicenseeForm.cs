using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewLicenseeForm : BackendPageBase
    {
        public NewLicenseeForm(IWebDriver driver) : base(driver)
        {
        }

        public string NameValidationMessage { get { return _driver.FindElementValue(By.XPath("//span[@data-bind='validationMessage: fields.name']")); } }

        public ViewLicenseeForm Submit(
            string licenseeName, 
            string companyName, 
            string contractStart, 
            string contractEnd, 
            string allowedBrands,
            string allowedWebsites,
            string email,
            string[] products = null,
            string[] currencies = null,
            string[] countries = null,
            string[] languages = null)
        {
            products = products ?? new[] { "Mock Sport Bets", "Mock Casino" };
            currencies = currencies ?? new[] { "CAD", "USD", "EUR", "GBP", "RMB" };
            countries = countries ?? new[] {"Canada", "China", "Great Britain", "United States"};
            languages = languages ?? new[] {"en-GB", "en-US", "zh-CN", "zh-TW"};

            var licenseeNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-name')]"));
            licenseeNameField.SendKeys(licenseeName);
            var companyNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-company-name')]"));
            companyNameField.SendKeys(companyName);
            var contractStartField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-contract-start')]"));
            contractStartField.SendKeys(contractStart);

            if (!string.IsNullOrEmpty(contractEnd))
            {
                var openEndedCheckbox = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-open-ended')]"));
                openEndedCheckbox.Click();

                var contractEndField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-contract-end')]"));
                contractEndField.SendKeys(contractEnd);
            }

            var emailField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-email')]"));
            _driver.FindElementScroll(By.XPath("//input[contains(@id, 'licensee-email')]"));
            emailField.SendKeys(email);
            var allowedBrandsField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-brand-count')]"));
            allowedBrandsField.SendKeys(allowedBrands);
            var allowedWebsitesField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'licensee-website-count')]"));
            allowedWebsitesField.SendKeys(allowedWebsites);

            //_driver.ScrollPage(0, 500);
            //_driver.FindElementScroll(By.XPath("//label[contains(@data-i18n, 'app:product.availableProducts')]"));
            products.ForEach(x => _driver.SelectFromMultiSelect("productsAssignControl", x));
            //_driver.FindElementScroll(By.XPath("//label[contains(@data-i18n, 'app:licensee.availableCurrencies')]"));
            currencies.ForEach(x => _driver.SelectFromMultiSelect("currenciesAssignControl", x));
            //_driver.ScrollPage(0, 700);
            //_driver.FindElementScroll(By.XPath("//label[contains(@data-i18n, 'app:licensee.availableCountries')]"));
            countries.ForEach(x => _driver.SelectFromMultiSelect("countriesAssignControl", x));
            //_driver.FindElementScroll(By.XPath("//label[contains(@data-i18n, 'app:licensee.availableLanguages')]"));
            languages.ForEach(x => _driver.SelectFromMultiSelect("languagesAssignControl", x));

            //_driver.ScrollPage(0, 2000);
            _driver.FindElementScroll(By.XPath(".//button[contains(@data-i18n, 'app:common.save')]"));
            var saveButton = _driver.FindElementWait(By.XPath(".//button[contains(@data-i18n, 'app:common.save')]"));
            saveButton.Click();
            var submittedForm = new ViewLicenseeForm(_driver);
            return submittedForm;
        }
    }
}