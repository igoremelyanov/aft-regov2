using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class RegisterPageStep2 : FrontendPageBase
    {
        #region Locators
        internal static readonly By SuccessMessageBy = By.XPath("//h2[contains(@data-i18n,'registration.nextStepDepositBelow')]");
        internal static readonly By InputAmountBy = By.Id("amount");
        internal static readonly By DepositButtonBy = By.XPath("//a[contains(@data-bind,'onSubmit')]");
        internal static readonly By ErrorMessageBy = By.XPath("//span[contains(@class,'validationMessage')]");
        
        internal static By DepositAmountButtonBy(String amount)
        {
            return By.XPath(  String.Format("//button[contains(text(),'{0}')]", amount));
        }
        #endregion

        #region Elements
        public IWebElement SuccessMessage
        {
            get { return _driver.FindElementWait(SuccessMessageBy); }
        }

        public IWebElement DepositAmountButton(string amount)
        {
            return _driver.FindElementWait(DepositAmountButtonBy(amount));
        }

        public IWebElement InputAmount
        {
            get { return _driver.FindElementWait(InputAmountBy); }
        }

        public IWebElement DepositButton
        {
            get { return _driver.FindElementWait(DepositButtonBy); }

        }

        public IWebElement ErrorMessage
        {
            get { return _driver.FindElementWait(ErrorMessageBy); }
        }
        #endregion

        public string GetSuccessMessage()
        {
            return SuccessMessage.Text;
        }

        public string GetErrorMessage()
        {
          return ErrorMessage.Text;
        }

        public void EnterDepositAmount(string amount)
        {
            _driver.WaitForElementClickable(InputAmount);
            InputAmount.SendKeys(Keys.Backspace + amount);
            _driver.WaitFieldIsEntered(InputAmount);
        }

        public void SelectDepositAmount(string amount)
        {
            var amountButton = DepositAmountButton(amount);
            
            _driver.WaitForElementClickable(amountButton);
            amountButton.Click();

            _driver.WaitFieldIsEntered(InputAmount);
        }

        public void SubmitOnlineDeposit()
        {
            _driver.ScrollToElement(DepositButton);
            _driver.WaitForElementClickable(DepositButton);
            DepositButton.Click();
            _driver.WaitForJavaScript();
        }

        public RegisterPageStep2(IWebDriver driver) : base(driver) { }
    }
}
