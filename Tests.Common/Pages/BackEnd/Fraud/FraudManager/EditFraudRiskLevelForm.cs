using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    public class EditFraudRiskLevelForm : BackendPageBase
    {
        internal static readonly By BrandBy = By.Id("fraud-form-brand-1");
        internal static readonly By InputFRLCodeBy = By.Id("fraud-form-level-1");
        internal static readonly By InputFRLNameBy = By.Id("fraud-form-name-1");
        internal static readonly By InputRemarksBy = By.Id("fraud-form-description-1");
        internal static readonly By SaveButtonBy = By.XPath("//button[contains(@data-bind,'save')]");
        internal static readonly By ValidationMessageBy = By.XPath("//div[contains(@class,'alert-danger')]");
        internal static readonly By SpecValidationMessageBy = By.XPath("//div/span[contains(@class,'validationMessage')]");

        public IWebElement SaveButton
        {
            get { return _driver.FindElementWait(SaveButtonBy); }
        }

        public IWebElement ValidationMessage
        {
            get { return _driver.FindElementWait(ValidationMessageBy); }
        }

        public IWebElement SpecValidationMessage
        {
            get { return _driver.FindElementWait(SpecValidationMessageBy); }
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


        public void EditFraudRiskLevelFields(FraudRiskLevelData data)
        {
   
            //Set Fraud Risk Level Name
            InputFRLName.Clear();
            InputFRLName.SendKeys(data.FRLName);

            //Enter Remarks 
            InputRemarks.Clear();
            InputRemarks.SendKeys(data.Remarks);
            
        }

        public ViewFraudRiskLevelForm SubmitFraudRiskLevel()
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(SaveButton);
            var page = new ViewFraudRiskLevelForm(_driver);
            page.Initialize();
            return page;
        }

        public EditFraudRiskLevelForm(IWebDriver driver)
            : base(driver)
        { }
    }
}
