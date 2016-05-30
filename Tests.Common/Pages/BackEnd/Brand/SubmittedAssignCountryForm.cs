using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedAssignCountryForm : BackendPageBase
    {
        public SubmittedAssignCountryForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//div[contains(@data-view, 'brand/country-manager/assign')]/div"));
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

        public bool IsCountryDisplayed(string country)
        {
            return _driver.FindElement(By.XPath(string.Format("//select[contains(@data-bind, 'options: assignedItems')]/option[text()='{0}']", country))).Displayed;
        }
    }
}