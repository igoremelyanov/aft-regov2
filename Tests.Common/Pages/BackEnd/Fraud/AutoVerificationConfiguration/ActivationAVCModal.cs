using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    //Represents Activation AVC modal
    public class ActivationAVCModal : ActivationDeactivationAVCDialog
    {
        internal static readonly By ActivateButtonBy = By.XPath("//button[contains(@data-i18n,'app:common.activate')]");
        internal static readonly By CancelButtonBy = By.XPath("//button[contains(@data-bind,'cancelClose')]");


        public ActivationDeactivationAvcConfirmationModal ConfirmAVCActivation()
        {
            Click(ActivateButtonBy);
            return new ActivationDeactivationAvcConfirmationModal(_driver);
        }

        public void CancelAVCActivation()
        {
           Click(CancelButtonBy);
        }

        public ActivationAVCModal(IWebDriver driver)
            : base(driver)
        { }
    }
}
