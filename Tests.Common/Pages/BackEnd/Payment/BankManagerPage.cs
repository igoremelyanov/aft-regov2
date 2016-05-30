using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment
{
    public class BankManagerPage : BackendPageBase
    {
        public const string GridXPath = "//div[contains(@data-view, 'payments/banks/list')]";

        public BankManagerPage(IWebDriver driver) : base(driver) {}

        public Grid Grid
        {
            get { return new Grid(_driver, GridXPath); }
        }

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//h5[text()='Banks']")); }
        }

        public NewBankForm OpenNewBankForm()
        {
            var newBankButton = FindActionButton("new", GridXPath);
            newBankButton.Click();
            var newBankForm = new NewBankForm(_driver);
            return newBankForm;
        }

        public SubmittedBankForm OpenViewBankForm(string bankName)
        {
            Grid.SelectRecord(bankName);
            var viewBankButton = FindActionButton("view", GridXPath);
            viewBankButton.Click();
            var viewBankForm = new SubmittedBankForm(_driver);
            return viewBankForm;
        }

        public EditBankForm OpenEditBankForm(string bankName)
        {
            Grid.SelectRecord(bankName);
            var editBankButton = FindActionButton("edit", GridXPath);
            editBankButton.Click();
            var editBankForm = new EditBankForm(_driver);
            return editBankForm;
        }
    }
}