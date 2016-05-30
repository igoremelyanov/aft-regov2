using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class DepositOnlinePage: CashierPage
    {
        #region Locators
        internal static readonly By InputAmountBy = By.Id("amount");
        internal static readonly By DepositButtonBy = By.XPath("//button[contains(., 'Deposit')]");
        internal static readonly By DepositConfirmedSumBy = By.XPath("//table[contains(@class,'tableinfo')]/tbody/tr[1]/td[2]");
        internal static readonly By BalanceAmountBy = By.XPath("//span[contains(@data-balance-type,'playable')]");
        internal static By DepositAmountButtonBy(String amount)
        { return By.XPath(String.Format("//button[contains(text(),'{0}')]", amount)); }
        #endregion

        #region Elements
        public IWebElement InputAmount
        {
            get { return _driver.FindElementWait(InputAmountBy); }
        }

        public IWebElement DepositAmountButton(string amount)
        {
            return _driver.FindElementWait(DepositAmountButtonBy(amount));
        }

        public IWebElement DepositButton
        {
            get { return _driver.FindElementWait(DepositButtonBy); }

        }

        public IWebElement DepositConfirmedSum
        {
            get { return _driver.FindElementWait(DepositConfirmedSumBy); }

        }
        #endregion

        public IWebElement BalanceAmount
        {
            get { return _driver.FindElementWait(BalanceAmountBy); }
        }

        public void EnterDepositAmount(string amount)
        {
            _driver.WaitForElementClickable(InputAmount);
            InputAmount.Clear();
            InputAmount.SendKeys(Keys.Backspace + amount + Keys.Tab);
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

        public string GetDepositConfirmedValue()
        {
           return DepositConfirmedSum.Text;
        }

        public string GetBalanceAmount()
        {
            return BalanceAmount.Text;
        }
        public DepositOnlinePage(IWebDriver driver) : base(driver) { }
    }
}
