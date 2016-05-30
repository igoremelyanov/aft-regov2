using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SupportedLanguagesPage : BackendPageBase
    {
        public SupportedLanguagesPage(IWebDriver driver) : base(driver)
        {
        }

        public AssignLanguageForm OpenAssignLanguageForm()
        {
            var assignLanguageButton = _driver.FindElementWait(By.Id("btn-assign-culture"));
            assignLanguageButton.Click();
            var form = new AssignLanguageForm(_driver);
            return form;
        }
    }

    public class AssignLanguageForm : BackendPageBase
    {
        public AssignLanguageForm(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedAssignLanguageForm Submit(string licensee, string brand, string[] language)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-culture-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-culture-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-culture-brand')]"), brand);

            language.ForEach(x => _driver.SelectFromMultiSelect("assignControl", x));

            var defaultLanguage = new SelectElement(_driver.FindElementWait(By.XPath("//div[contains(@data-bind, 'with: form.fields.defaultCulture')]/select")));
            defaultLanguage.SelectByText(language[0]);

            var saveButton = _driver.FindElementWait(By.XPath("//div[@data-view='brand/culture-manager/assign']//button[text()='Save']"));
            saveButton.Click();
            var form = new SubmittedAssignLanguageForm(_driver);
            return form;
        }

        public SubmittedAssignLanguageForm SubmitUnassign(string licensee, string brand, string[] language)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-culture-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-culture-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-culture-brand')]"), brand);

            language.ForEach(x => _driver.SelectFromMultiSelectUnassign("assignControl", x));            

            var saveButton = _driver.FindElementWait(By.XPath("//div[@data-view='brand/culture-manager/assign']//button[text()='Save']"));
            saveButton.Click();
            var form = new SubmittedAssignLanguageForm(_driver);
            return form;
        }

        public bool IsUnassignButtonDisplayed(string licensee, string brand)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-culture-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-culture-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-culture-brand')]"), brand);

            var unassignButton = _driver.FindElement(By.XPath("//button[contains(@data-bind, 'click: unassign')]")).Displayed;
            return unassignButton;
        }

        public bool IsDefaultLanguageDropdownBoxDisplayed(string licensee, string brand)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-culture-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-culture-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-culture-brand')]"), brand);

            var dropdownBox = _driver.FindElement(By.XPath("//div[contains(@data-bind, 'with: form.fields.defaultCulture')]/select")).Displayed;
            return dropdownBox;
        }
    }

    public class SubmittedAssignLanguageForm : BackendPageBase
    {
        public SubmittedAssignLanguageForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            }
        }

        public string Licensee
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-bind='with: form.fields.licensee']/p")); }
        }

        public string Brand
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-bind='with: form.fields.brand']/p")); }
        }

        public string Language(string language)
        {

            var languageValue = string.Format("//select[contains(@data-bind, 'options: assignedItems')]/option[text()='{0}']", language);
            return _driver.FindElementValue(By.XPath(languageValue));
        }

        public string DefaultLanguage { get{ return _driver.FindElementValue(By.XPath("//div[@data-bind='with: form.fields.defaultCulture']/p")); } }
    }
}