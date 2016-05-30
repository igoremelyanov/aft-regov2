using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    public class ViewFraudRiskLevelForm : BackendPageBase
    {
        internal static readonly By SuccessAlertBy = By.XPath("//div[contains(@class,'alert-success')]");
        internal static readonly By LicenseeBy = By.XPath("//p[contains(@data-bind,'licensees()')]");
        internal static readonly By BrandBy = By.XPath("//p[contains(@data-bind,'brands()')]");
        internal static readonly By FRLCodeBy = By.XPath("//p[contains(@data-bind,'fields.level')]");
        internal static readonly By FRLNameBy = By.XPath("//p[contains(@data-bind,'fields.name')]");
        internal static readonly By RemarksBy = By.XPath("//p[contains(@data-bind,'fields.description')]");


        #region General fields

        public IWebElement SuccessAlert
        {
            get { return _driver.FindElementWait(SuccessAlertBy); }
        }

        public IWebElement Licensee
        {
            get { return _driver.FindElementWait(LicenseeBy); }
        }


        public IWebElement Brand
        {
            get { return _driver.FindElementWait(BrandBy); }
        }

        public IWebElement FRLCode
        {
            get { return _driver.FindElementWait(FRLCodeBy); }
        }

        public IWebElement FRLName
        {
            get { return _driver.FindElementWait(FRLNameBy); }
        }

        public IWebElement Remarks
        {
            get { return _driver.FindElementWait(RemarksBy); }
        }

        #endregion

        public ViewFraudRiskLevelForm(IWebDriver driver)
            : base(driver)
        { }
    }
}
