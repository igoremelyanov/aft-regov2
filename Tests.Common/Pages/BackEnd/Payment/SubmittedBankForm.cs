using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment
{
    public class SubmittedBankForm : BackendPageBase
    {
        public SubmittedBankForm(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public string BankNameValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.name')]")); }
        }

        public string LicenseeValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.licenseeName')]")); }
        }

        public string BrandValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.brandName')]")); }
        }

        public string BankIdValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.bankId')]")); }
        }

        public string CountryValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'Model.country')]")); }
        }

        public string RemarksValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'Model.remarks')]")); }
        }
    }
}