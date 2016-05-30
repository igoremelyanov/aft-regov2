using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    public class NewFraudRiskLevelForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy =
            By.XPath("//button[contains(@type,'submit')]");
        internal static readonly By ValidationMessageBy = By.XPath("//div[contains(@class,'alert-danger')]");

        internal static readonly By LicenseeBy = By.Id("fraud-form-licensee-1");
        internal static readonly By BrandBy = By.Id("fraud-form-brand-1");
        internal static readonly By InputFRLCodeBy = By.Id("fraud-form-level-1");
        internal static readonly By InputFRLNameBy = By.Id("fraud-form-name-1");
        internal static readonly By InputRemarksBy = By.Id("fraud-form-description-1");


        public IWebElement ValidationMessage
        {
            get { return _driver.FindElementWait(ValidationMessageBy); }
        }

        public IWebElement InputFRLCode
        {
            get { return _driver.FindElementWait(InputFRLCodeBy); }
        }

        public IWebElement InputFRLName
        {
            get { return _driver.FindElementWait(InputFRLNameBy); }
        }

        public IWebElement InputRemarks
        {
            get { return _driver.FindElementWait(InputRemarksBy); }
        }

        public void SetFraudRiskLevelFields(FraudRiskLevelData data)
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

           //Set Fraud Risk Level Code
            InputFRLCode.Clear();
            InputFRLCode.SendKeys(data.FRLCode);

           //Set Fraud Risk Level Name
            InputFRLName.Clear();
            InputFRLName.SendKeys(data.FRLName);

           //Enter Remarks 
            InputRemarks.Clear();
            InputRemarks.SendKeys(data.Remarks);
         }

        public ViewFraudRiskLevelForm SubmitFraudRiskLevel()
        {
            Click(SaveButtonBy);
            var page = new ViewFraudRiskLevelForm(_driver);
            page.Initialize();
            return page;
        }


        public NewFraudRiskLevelForm(IWebDriver driver)
            : base(driver)
        {}
    }

}
