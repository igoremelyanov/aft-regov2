using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration
{    /// <summary>
    /// Represents Fraud -> Auto Verification Configuration page 
    /// </summary>
    public class AutoVerificationConfigurationPage : BackendPageBase
    {
        internal static readonly By NewButtonBy = By.XPath("//button[contains(@name,'add')]");
        internal static readonly By EditButtonBy = By.XPath("//button[contains(@name,'edit')]");
        internal static readonly By ActivateButtonBy = By.XPath("//button[contains(@name,'activate')]");
        internal static readonly By DeactivateButtonBy = By.XPath("//button[contains(@name,'deactivate')]");
        internal static readonly By AVCStatusBy = By.XPath(".//td[contains(@aria-describedby,'Status')]");
        internal static readonly By ScrolGridDropdown = By.XPath("//select[contains(@class,'ui-pg-selbox')]");

        public IWebElement ActivateButton
        {
            get { return _driver.FindElementWait(ActivateButtonBy); }
        }

        //Open new auto verification form
        public NewAutoVerificationConfigurationForm OpenNewAutoVerificationForm()
        {
            Click(NewButtonBy);
            var page = new NewAutoVerificationConfigurationForm(_driver);
            _driver.WaitForJavaScript();
            page.Initialize();
            
            return page;
        }

        //Select record (auto verification form) by key values
        public IWebElement SelectAvcRecord(AutoVerificationConfigurationData avcData)
        {
            new SelectElement(_driver.FindElementWait(ScrolGridDropdown)).SelectByText("100");
            _driver.WaitForJavaScript();
            
            var recordXPath =
                string.Format(
                    "//table//tr[contains(., '{0}') and contains(., '{1}') and contains(., '{2}') and contains(., '{3}')]",
                    avcData.Licensee, avcData.Brand, avcData.Currency, avcData.VipLevel);

            var element = _driver.FindElementWait(By.XPath(recordXPath));
            element.Click();

            return element;
        }

        //Open edit AVC form for the selected record
        public EditAutoVerificationConfigurationForm OpenEditAutoVerificationConfigurationForm(AutoVerificationConfigurationData avcData)
        {
            SelectAvcRecord(avcData);
            Click(EditButtonBy);
            var form = new EditAutoVerificationConfigurationForm(_driver);
            form.Initialize();
            return form;
        }

        private ActivationAVCModal OpenActivationAVCModal(AutoVerificationConfigurationData avcData)
        {
            SelectAvcRecord(avcData);
            _driver.WaitAndClickElement(ActivateButton);
            var activationModal = new ActivationAVCModal(_driver);
            return activationModal;
        }

        //Activate Auto Verification Configuration
        public ActivationDeactivationAvcConfirmationModal ActivateAutoVerificationConfiguration(AutoVerificationConfigurationData avcData)
        {
            var activationModal = OpenActivationAVCModal(avcData);
            activationModal.EnterRemark("Activate " + Guid.NewGuid());
            return activationModal.ConfirmAVCActivation();
        }

        //Cancel Activation of Auto Verification Configuration
        public void CancelActivationAutoVerificationConfiguration(AutoVerificationConfigurationData avcData)
        {   
            OpenActivationAVCModal(avcData).CancelAVCActivation();
        }

        //Deactivate Auto Verification Configuration
        public ActivationDeactivationAvcConfirmationModal DeactivateAutoVerificationConfiguration(AutoVerificationConfigurationData avcData)
        {
            SelectAvcRecord(avcData);
            Click(DeactivateButtonBy);
            var deactivationModal = new DeactivationAvcModal(_driver);
            deactivationModal.EnterRemark("Deactivate "+ Guid.NewGuid());
            return deactivationModal.ConfirmAVCDeactivation();
        }

        public string GetAVCStatus(AutoVerificationConfigurationData avcData)
        {
            var record = SelectAvcRecord(avcData);
            return record.FindElement(AVCStatusBy).Text;
        }

        public AutoVerificationConfigurationPage(IWebDriver driver)
            : base(driver)
        { }
    }
}
