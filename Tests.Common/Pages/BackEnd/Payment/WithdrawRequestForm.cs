using System;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class WithdrawRequestForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy = By.XPath("//button[contains(@type,'submit')]");
        internal static readonly By InputAmountBy = By.XPath("//input[contains(@data-bind,'amountFieldId')]");
        internal static readonly By InputRemarksBy = By.XPath("//textarea[contains(@data-bind,'remarksFieldId')]");


        public WithdrawRequestForm(IWebDriver driver) : base(driver)
        {
        }


        public IWebElement SaveButton
        {
            get { return _driver.FindElementWait(SaveButtonBy); }
        }

        public IWebElement InputAmount
        {
            get { return _driver.FindElementWait(InputAmountBy); }
        }

        public IWebElement InputRemarks
        {
            get { return _driver.FindElementWait(InputRemarksBy); }
        }

        public ViewOfflineWithdrawRequest  Submit()
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(SaveButton);
            var page = new ViewOfflineWithdrawRequest(_driver);
            page.Initialize();
            return page;
        }

        public ViewOfflineWithdrawRequest SetOfflineWithdrawRequest (OfflineWithdrawRequestData data)
        {   
            InputAmount.Clear();
            InputAmount.SendKeys(data.Amount);
            InputRemarks.Clear();
            InputRemarks.SendKeys(data.Remarks);

            return Submit();
         }

        public void TryToSubmit(string amount, NotificationMethod notificationMethod)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'withdraw-request-amount')]"));
            wait.Until(d => provinceField.Displayed && provinceField.Enabled);

            _amount.SendKeys(amount);
            _remarks.SendKeys(TestDataGenerator.GetRandomString(5));

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            _driver.FindElementScroll(By.XPath("//button[text()='Save']"));
            saveButton.Click();
        }

        public string ValidationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-view, 'payments/withdrawal/request')]/div")); }
        }

        public string BankNameValue { get{ return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bank']")); } }
        public string ProvinceValue { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankProvince']")); } }
        public string CityValue { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankCity']")); } }
        public string Branch { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankBranch']")); } }
        public string SwiftCode { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankSwiftCode']")); } }
        public string Address { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankAddress']")); } }
        public string BankAccountNameValue { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankAccountName']")); } }
        public string BankAccountNumberValue { get { return _driver.FindElementValue(By.XPath("//*[@data-bind='text: bankAccountNumber']")); } }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@id, 'withdraw-request-amount')]")]
        private IWebElement _amount;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@id, 'withdraw-request-remarks')]")]
        private IWebElement _remarks;
#pragma warning restore 649

    }
}