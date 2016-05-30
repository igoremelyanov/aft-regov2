using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    /// <summary>
    /// Represents Activation Auto Configuration Confirmation modal window
    /// </summary>
    public class ActivationDeactivationAvcConfirmationModal : BackendPageBase
    {
        internal static readonly By SuccessAlertBy = By.XPath("//div[contains(@class,'alert-success')]");
        internal static readonly By CloseButtonBy = By.XPath("//button[contains(@data-bind,'cancelClose')]");
      
        public IWebElement SuccessAlert
        {
            get { return _driver.FindElementWait(SuccessAlertBy); }
        }

        public void CloseConfirmationModal()
        {
            Click(CloseButtonBy);
        }

        public ActivationDeactivationAvcConfirmationModal(IWebDriver driver)
            : base(driver)
        { }
    }
}
