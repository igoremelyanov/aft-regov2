using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BankAccountManagerPage : BackendPageBase
    {
        public BankAccountManagerPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "bank-accounts-name-search", "bank-accounts-search-button");
            }
        }

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//h5[text()='Bank Accounts']")); }
            
        }

        public NewBankAccountForm OpenNewBankAccountForm()
        {
            var newButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/list']//button[contains(@data-bind, 'click: openAddTab')]"));
            newButton.Click();
            var form = new NewBankAccountForm(_driver);
            form.Initialize();
            return form;
        }       

        public SubmittedBankAccountForm OpenViewBankAccountForm(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            var viewButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'openViewTab')]"));
            viewButton.Click();
            var form = new SubmittedBankAccountForm(_driver);
            return form;
        }

        public EditBankAccountForm OpenEditForm(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            var editButton = _driver.FindElementWait(By.Id("bank-accounts-edit-btn"));
            editButton.Click();
            var form = new EditBankAccountForm(_driver);
            form.Initialize();
            return form;
        }

        public ActivateBankAccountDialog OpenActivateBankAccountDialog(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            var activateButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/list']//button[contains(@data-bind, 'enable: canActivate')]"));
            activateButton.Click();
            var dialog = new ActivateBankAccountDialog(_driver);
            return dialog;
        }

        public DeactivateBankAccountDialog OpenDeactivateBankAccountDialog(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            var deactivateButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'canDeactivate')]"));
            deactivateButton.Click();
            var dialog = new DeactivateBankAccountDialog(_driver);
            return dialog;
        }

        public string Status { get { return _driver.FindElementScroll(By.XPath("//*[contains(@aria-describedby,'Status')]")).Text; } }
    }
}