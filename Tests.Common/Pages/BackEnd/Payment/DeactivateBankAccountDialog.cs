using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment
{
    public class DeactivateBankAccountDialog : BackendPageBase
    {
        public DeactivateBankAccountDialog(IWebDriver driver) : base(driver) { }

        public SubmittedActivateBankAccountDialog DeactivateBankAccount(string remark)
        {
            var remarkField = _driver.FindElementWait(By.XPath("//textarea[@data-bind='value: remarks']"));
            remarkField.SendKeys(remark);
            var deactivateButton = _driver.FindElementWait(By.XPath("//button[text()='Deactivate']"));
            deactivateButton.Click();
            var submittedDialog = new SubmittedActivateBankAccountDialog(_driver);
            return submittedDialog;
        }
    }
}
