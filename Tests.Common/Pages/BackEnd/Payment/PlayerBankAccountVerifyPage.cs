using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerBankAccountVerifyPage : BackendPageBase
    {
        private const string BaseDialogXpath = "//div[@data-view='payments/player-bank-accounts/status-dialog']";

        public const string GridXPath = "//div[contains(@data-view, 'payments/player-bank-accounts/pending-list')]";

        public Grid Grid { get { return new Grid(_driver, GridXPath); } }

        public static By RemarksXpath = By.XPath(BaseDialogXpath + "//textarea[contains(@data-bind, 'value: remarks')]");
        public static By ConfirmButtonXpath = By.XPath(BaseDialogXpath + "//button[contains(@data-bind, 'click: changeStatus')]");
        public static By CloseButtonXpath = By.XPath(BaseDialogXpath + "//button[contains(@data-bind, 'click: close')]");

        public PlayerBankAccountVerifyPage(IWebDriver driver) : base(driver) { }

        public ViewPlayerBankAccountVerify OpenViewForm(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            _driver.WaitForJavaScript();
            var viewButton = _driver.FindElement(By.XPath("//button[@name='view']"));
            _driver.WaitForElementClickable(viewButton);
            _driver.WaitAndClickElement(viewButton);
            return new ViewPlayerBankAccountVerify(_driver);
        }

        public string VerifyAndReturnMessage(string bankAccountName, string remarks = "player's bank account verified")
        {
            Grid.SelectRecord(bankAccountName);
            var verifyButton = FindActionButton("verify", GridXPath);
            verifyButton.Click();

            var remarksField = _driver.FindElementWait(RemarksXpath);
            remarksField.SendKeys(remarks);

            var confirmButton = _driver.FindElementWait(ConfirmButtonXpath);
            confirmButton.Click();

            var _successMessage = _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'successMessage')]"));

            return _successMessage;
        }

        public void CloseDialog()
        {
            var closeButton = _driver.FindElementWait(CloseButtonXpath);
            closeButton.Click();
        }

        public void Verify(string bankAccountName, string remarks = null)
        {
            Grid.SelectRecord(bankAccountName);
            var verifyButton = FindActionButton("verify", GridXPath);
            verifyButton.Click();

            if (!string.IsNullOrEmpty(remarks))
            {
                var remarksField = _driver.FindElementWait(RemarksXpath);
                remarksField.SendKeys(remarks);
            }

            var confirmButton = _driver.FindElementWait(ConfirmButtonXpath);
            confirmButton.Click();

            var closeButton = _driver.FindElementWait(CloseButtonXpath);
            closeButton.Click();
        }

        public string RejectAndReturnMessage(string bankAccountName, string remarks = "player's bank account rejected")
        {
            Grid.SelectRecord(bankAccountName);
            var rejectButton = FindActionButton("reject", GridXPath);
            rejectButton.Click();

            var remarksField = _driver.FindElementWait(RemarksXpath);
            remarksField.SendKeys(remarks);

            var confirmButton = _driver.FindElementWait(ConfirmButtonXpath);
            confirmButton.Click();

            var _successMessage = _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'successMessage')]"));

            return _successMessage;
        }
    }
}
