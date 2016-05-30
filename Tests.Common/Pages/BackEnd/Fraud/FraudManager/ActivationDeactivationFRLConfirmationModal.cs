
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    /// <summary>
    /// Represents Activation FRL Confirmation modal window
    /// </summary>
    public class ActivationDeactivationFRLConfirmationModal : BackendPageBase
    {
        internal static readonly By SuccessAlertBy = By.XPath("//div[contains(@class,'alert-success')]");
        internal static readonly By CloseButtonBy = By.XPath("//button[contains(@data-bind,'close')]");
      
        public IWebElement SuccessAlert
        {
            get { return _driver.FindElementWait(SuccessAlertBy); }
        }

        public IWebElement CloseButton
        {
            get { return _driver.FindElementWait(CloseButtonBy); }
        }

        public void CloseConfirmationModal()
        {
             var closeButton = CloseButton;
             closeButton.Click();
             _driver.WaitForElementInvisible(closeButton);
        }

        public ActivationDeactivationFRLConfirmationModal(IWebDriver driver)
            : base(driver)
        { }
    }
}
