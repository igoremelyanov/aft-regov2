using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration
{
    public class DeactivationAvcModal : ActivationDeactivationAVCDialog
    {
        internal static readonly By DeActivateButtonBy = By.XPath("//button[contains(@data-i18n,'app:common.deactivate')]");

        public ActivationDeactivationAvcConfirmationModal ConfirmAVCDeactivation()
        {
            Click(DeActivateButtonBy);
            return new ActivationDeactivationAvcConfirmationModal(_driver);
        }

        public DeactivationAvcModal(IWebDriver driver)
            : base(driver)
        { }
    }
}
