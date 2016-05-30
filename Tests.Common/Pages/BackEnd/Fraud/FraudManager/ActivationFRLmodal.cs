using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager
{
    //Represents Activation FRL modal
    public class ActivationFRLModal : ActivationDeactivationFRLDialog
    {
        internal static readonly By ActivateButtonBy = By.XPath("//button[contains(@data-bind,'ok')]");
        internal static readonly By CancelButtonBy = By.XPath("//button[contains(@data-bind,'close')]");

        public IWebElement CancelButton
        {
            get { return _driver.FindElementWait(CancelButtonBy); }
        }


        public ActivationDeactivationFRLConfirmationModal ConfirmFRLActivation()
        {
            Click(ActivateButtonBy);
            return new ActivationDeactivationFRLConfirmationModal(_driver);
        }

        public void CancelFRLActivation()
        {
            var cancelButton = CancelButton;
            _driver.WaitAndClickElement(cancelButton);
            _driver.WaitForElementInvisible(cancelButton);
        }

        public ActivationFRLModal(IWebDriver driver)
            : base(driver)
        { }
    }
}
