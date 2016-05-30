using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class OfflineDepositRequestPage : FrontendPageBase
    {

        #region Locators
        internal static readonly By InputAmountBy = By.Id("amount");
        internal static readonly By InputBonusCodeBy = By.XPath("//input[@data-bind='value: code, enable: amountEntered']");
        internal static readonly By ButtonSubmitDepositBy = By.XPath("//button[contains(., 'Deposit')]");
        internal static readonly By InputRemarkBy = By.XPath("//input[@data-bind='value: remark']");
        internal static readonly By ConfirmationMessageBy = By.XPath("//p[@data-i18n='cashier.depositCongratulation']");
        
        #endregion

        #region Elements
        public IWebElement InputAmount
        {
            get { return _driver.FindElementWait(InputAmountBy); }
        }

        public IWebElement InputBonusCode
        {
            get { return _driver.FindElementWait(InputBonusCodeBy); }
        }

        public IWebElement ButtonSubmitDeposit
        {
            get { return _driver.FindElementWait(ButtonSubmitDepositBy); }
        }

        public IWebElement InputRemark
        {
            get { return _driver.FindElementWait(InputRemarkBy); }
        }

        public IWebElement ConfirmationMessage
        {
            get { return _driver.FindElementWait(ConfirmationMessageBy); }
        }
        #endregion

        public OfflineDepositRequestPage(IWebDriver driver) : base(driver) {}

        public String GetConfirmationMessage()
        {
            return ConfirmationMessage.Text;
        }
        
        public void Submit(string amount, string playerRemark, string bankName = null, string bonusCode = null)
        {
            if (bankName != null)
            {
                var bankField = _driver.FindElementWait(By.XPath("//select[@data-bind='value: bankAccount']"));
                var bankList = new SelectElement(bankField);
                bankList.SelectByText(bankName);
            }
                       
            InputAmount.Clear();
            InputAmount.SendKeys(amount);
                        
            InputRemark.SendKeys(playerRemark);

            if (bonusCode != null)
            {
                InputBonusCode.SendKeys(bonusCode);
            }

            ButtonSubmitDeposit.Click();
        }

    }
}