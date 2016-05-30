using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SetPaymentLevelPage : BackendPageBase
    {
        public static By PaymentLevelSelect = By.XPath("//select[contains(@data-bind, 'options: paymentLevels')]");
        public static By Remarks = By.XPath("//textarea[@data-bind='value: remarks']");

        public static By SubmitButton =
            By.XPath("//button[contains(@data-bind, 'click: save')]");

        public static By CloseButton = By.XPath("//button[contains(@data-bind, 'click: close')]");

        public static By ConfirmYes = By.XPath("//div[contains(@data-view, 'plugins/messageBox')]//button[text()='Yes']");

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public SetPaymentLevelPage(IWebDriver driver) : base(driver)
        {
        }

        public void Submit(string newPaymentLevel, string remarks)
        {
            var selectList = _driver.FindElementWait(PaymentLevelSelect);
            var selectElement = new SelectElement(selectList);
            selectElement.SelectByText(newPaymentLevel);

            if (remarks != null)
            {
                var remarksField = _driver.FindElementWait(Remarks);
                remarksField.Clear();
                remarksField.SendKeys(remarks);
            }

            var saveButton = _driver.FindElementWait(SubmitButton);
            saveButton.Click();

            var confirmYesButton = _driver.FindElementWait(ConfirmYes);
            confirmYesButton.Click();
        }

        public void Close(string newPaymentLevel, string remarks)
        {
            var closeButton = _driver.FindElementWait(CloseButton);
            closeButton.Click();
        }
    }
}