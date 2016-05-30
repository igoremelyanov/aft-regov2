using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration
{
    public class NewAutoVerificationConfigurationForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy =
            By.XPath("//button[contains(@class,'btn') and contains(@data-i18n,'save')]");

        internal static readonly By LicenseeBy = By.Id("verification-licensee");
        internal static readonly By BrandBy = By.Id("verification-brand");
        internal static readonly By VipLevelBy = By.XPath("//div[contains(@data-bind, 'form.fields.vipLevels.value')]");
        internal static readonly By CurrencyContainerBy = By.Id("verification-currency");

        public IWebElement SaveButton
        {
            get { return _driver.FindElementWait(SaveButtonBy); }
        }

        public ViewAutoVerificationConfigurationForm SubmitAutoVerificationConfiguration()
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(SaveButton);
            var page = new ViewAutoVerificationConfigurationForm(_driver);
            page.Initialize();
            return page;
        }

        public void SetAutoVerificationConfigurationFields(AutoVerificationConfigurationData data)
        {
            //Set licensee
            IWebElement licenseeDropdown = null;
            try
            {
                var e = _driver.FindElement(LicenseeBy);
                if (e.Enabled)
                    licenseeDropdown = e;
            }
            catch (NoSuchElementException)
            {
            }
          
            if (licenseeDropdown != null)
                new SelectElement(licenseeDropdown).SelectByText(data.Licensee);

            //Set Brand
            new SelectElement(_driver.FindElementWait(BrandBy)).SelectByText(data.Brand);

            //Set Currency
            new SelectElement(_driver.FindElementWait(CurrencyContainerBy)).SelectByText(data.Currency);
  
            //Set VIP level
            new MultiSelectWidget(_driver, VipLevelBy).SelectFromMultiSelect(data.VipLevel);
        }

        public NewAutoVerificationConfigurationForm(IWebDriver driver)
            : base(driver)
        {}

    }

}