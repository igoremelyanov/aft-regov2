/// <summary>
/// Represents Activation FRL modal window
/// </summary>

using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    public class ActivationDeactivationFRLDialog : BackendPageBase
    {
        internal static readonly By RemarksBy = By.Id("remarks");

        public IWebElement Remarks
        {
            get { return _driver.FindElementWait(RemarksBy); }
        }

        public void EnterRemark(string remark)
        {
            Remarks.Clear();
            Remarks.SendKeys(remark);
        }

        public ActivationDeactivationFRLDialog(IWebDriver driver)
            : base(driver)
        { }
    }


}
