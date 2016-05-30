using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration
{
    /// <summary>
    /// Represents Activation Auto Configuration modal window
    /// </summary>
    public class ActivationDeactivationAVCDialog : BackendPageBase
    {
        internal static readonly By RemarksBy = By.XPath("//div/textarea[contains(@data-bind,'remarks')]");


        public IWebElement Remarks
        {
            get { return _driver.FindElementWait(RemarksBy); }
        }

        public void EnterRemark (string remark)
        {
            Remarks.Clear();
            Remarks.SendKeys(remark);
        }


        public ActivationDeactivationAVCDialog(IWebDriver driver)
            : base(driver)
        { }
    }
}
