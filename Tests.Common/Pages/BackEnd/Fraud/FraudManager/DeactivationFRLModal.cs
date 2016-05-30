using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    public class DeactivationFRLModal : ActivationDeactivationFRLDialog
    {
        internal static readonly By DeActivateButtonBy = By.XPath("//button[contains(@data-bind,'ok')]");

        public ActivationDeactivationFRLConfirmationModal ConfirmFRLDeactivation()
        {
            Click(DeActivateButtonBy);
            return new ActivationDeactivationFRLConfirmationModal(_driver);
        }

        public DeactivationFRLModal(IWebDriver driver)
            : base(driver)
        { }
    }
}
