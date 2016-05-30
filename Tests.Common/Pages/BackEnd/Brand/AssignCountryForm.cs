using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;


namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class AssignCountryForm : BackendPageBase
    {
        public AssignCountryForm(IWebDriver driver) : base(driver)
        {
        }

        private const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
            "/div/div[@data-view='brand/country-manager/assign']";


        public SubmittedAssignCountryForm AssignCountries(string licensee, string brand, string[] country)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-country-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-country-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-country-brand')]"), brand);

            country.ForEach(x => _driver.SelectFromMultiSelect("assignControl", x));

            var saveButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedAssignCountryForm(_driver);
            return submittedForm;
        }

        public SubmittedAssignCountryForm UnassignCountries(string licensee, string brand, string[] country)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-country-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-country-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-country-brand')]"), brand);

            country.ForEach(x => _driver.SelectFromMultiSelectUnassign("assignControl", x));

            var saveButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedAssignCountryForm(_driver);
            return submittedForm;
        }
    }
}