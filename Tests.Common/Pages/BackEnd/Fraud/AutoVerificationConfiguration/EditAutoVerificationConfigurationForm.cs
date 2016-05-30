using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration
{
    public class EditAutoVerificationConfigurationForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy = By.XPath("//button[contains(@type,'submit')]");
        internal static readonly By LicenseeBy = By.Id("verification-licensee");
        internal static readonly By BrandBy = By.Id("verification-brand");
        internal static readonly By VipLevelBy = By.XPath("//div[contains(@data-bind, 'form.fields.vipLevels.value')]");
        internal static readonly By CurrencyContainerBy = By.Id("verification-currency");
        internal static readonly By ValidationMessageBy = By.XPath("//span[contains(@data-bind,'validationMessage')]");


        //Total Deposit amount check
        internal static readonly By CheckBoxTotalDepositAmountBy = By.Id("verification-has-total-deposit-amount");
        internal static readonly By DropdownTotalDepositAmountBy = By.XPath("//select[contains(@data-bind,'form.fields.hasTotalDepositAmount.value')]");
        internal static readonly By InputTotalDepositAmountBy = By.XPath("//input[contains(@data-bind,'form.fields.totalDepositAmount.value')]");

        //Account age
        internal static readonly By CheckBoxAccountAgeBy = By.Id("verification-has-account-age");
        internal static readonly By DropdownAccountAgeBy = By.XPath("//select[contains(@data-bind,'form.fields.hasAccountAge.value()')]");
        internal static readonly By InputDaysBy = By.XPath("//input[contains(@data-bind,'form.fields.accountAge.value')]");

        //Deposit count
        internal static readonly By CheckBoxDepositCountBy = By.Id("verification-has-deposit-count");
        internal static readonly By DropDownDepositCountBy = By.XPath("//select[contains(@data-bind,'form.fields.hasDepositCount.value')]");
        internal static readonly By InputDepositCountBy = By.XPath("//input[contains(@data-bind,'form.fields.totalDepositCountAmount.value')]");

        //Withdrawal count
        internal static readonly By CheckBoxWithdrawalCountBy = By.Id("verification-has-withdrawal-count");
        internal static readonly By DropDownWithdrawalCountBy = By.XPath("//select[contains(@data-bind,'form.fields.hasWithdrawalCount.value()')]");
        internal static readonly By InputWithdrawalCountBy = By.XPath("//input[contains(@data-bind,'form.fields.totalWithdrawalCountAmount.value')]");

        //Win Loss
        internal static readonly By CheckBoxWinLossBy = By.Id("verification-has-win-loss");
        internal static readonly By DropDownWinLossBy = By.XPath("//select[contains(@data-bind,'form.fields.hasWinLoss.value')]");
        internal static readonly By InputWinLossAmountBy = By.XPath("//input[contains(@data-bind,'form.fields.winLossAmount.value')]");

        //Has payment level
        internal static readonly By CheckBoxHasPaymentLevelBy = By.Id("verification-has-payment-level");
        internal static readonly By AvailableItemBy = By.XPath("//select[contains(@data-bind, 'selectedAvailableItems, enable: $root.form.fields.hasPaymentLevel')]/option");
        internal static readonly By AssignPaymentLevelBy = By.XPath("//div[contains(@data-bind,'PaymentLevel')]//button[starts-with(@data-bind, 'click: assign')]");
        internal static readonly By UnassignPaymentLevelBy = By.XPath("//div[contains(@data-bind,'PaymentLevel')]//button[starts-with(@data-bind, 'click: unassign')]");

        public IWebElement ValidationMessage
        {
            get { return _driver.FindElementWait(ValidationMessageBy); }
        }

        public IWebElement SaveButton
        {
            get { return _driver.FindElementWait(SaveButtonBy); }
        }

        public void EditAutoVerificationConfigurationFields(AutoVerificationConfigurationData data, AutoVerificationConfigurationData dataEdited)
        {
            //Set Currency
            new SelectElement(_driver.FindElementWait(CurrencyContainerBy)).SelectByText(dataEdited.Currency);

            //Set VIP level
            new MultiSelectWidget(_driver, VipLevelBy).DeselectFromMultiSelect(data.VipLevel);
            new MultiSelectWidget(_driver, VipLevelBy).SelectFromMultiSelect(dataEdited.VipLevel);

        }

        #region Total Deposit Amount

        public string DropdownTotalDepositAmountSelected
        {
            get { return new SelectElement(_driver.FindElementWait(DropdownTotalDepositAmountBy)).SelectedOption.Text; }
        }

        public string TotalDepositAmountValue
        {
            get { return _driver.FindElement(InputTotalDepositAmountBy).GetAttribute("value"); }
        }

        public IWebElement InputTotalDepositAmount
        {
            get { return _driver.FindElementWait(InputTotalDepositAmountBy); }
        }

        public void SetTotalDepositAmountCheck(string criteria, string amount)
        {
            Click(CheckBoxTotalDepositAmountBy);
            new SelectElement(_driver.FindElementWait(DropdownTotalDepositAmountBy)).SelectByText(criteria);
            InputTotalDepositAmount.Clear();
            InputTotalDepositAmount.SendKeys(Keys.Backspace + amount);
        }

        public ViewAutoVerificationConfigurationForm UnchecklDepositAmount()
        {
            Click(CheckBoxTotalDepositAmountBy);
            return Submit();
        }

        #endregion

        #region WinLoss

        public IWebElement CheckBoxWinLoss
        {
            get { return _driver.FindElementWait(CheckBoxWinLossBy); }
        }

        public IWebElement DropdownWinLoss
        {
            get { return _driver.FindElementWait(DropDownWinLossBy); }
        }

        public string DropdownWinLossSelected
        {
            get { return new SelectElement(_driver.FindElementWait(DropDownWinLossBy)).SelectedOption.Text; }
        }

        public string WinLossAmountValue
        {
            get { return _driver.FindElement(InputWinLossAmountBy).GetAttribute("value"); }
        }

        public IWebElement InputWinLossAmount
        {
            get { return _driver.FindElementWait(InputWinLossAmountBy); }
        }

        public void SetWinLossCheck(string criteria, string amount)
        {
           
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(CheckBoxWinLoss);
            _driver.WaitForElementClickable(DropdownWinLoss);
            _driver.ScrollingToBottomofAPage();
            new SelectElement(_driver.FindElementWait(DropDownWinLossBy)).SelectByText(criteria);
            InputWinLossAmount.Clear();
            InputWinLossAmount.SendKeys(Keys.Backspace + amount);

        }

        public ViewAutoVerificationConfigurationForm UncheckWinLoss()
        {
            Click(CheckBoxWinLossBy);
            return Submit();
        }

        #endregion

        #region Withdrawal Count

        public IWebElement CheckBoxWithdrawalCount
        {
            get { return _driver.FindElementWait(CheckBoxWithdrawalCountBy); }
        }

        public IWebElement DropDownWithdrawalCount
        {
            get { return _driver.FindElementWait(DropDownWithdrawalCountBy); }
        }

        public string DropDownWithdrawalCountSelected
        {
            get { return new SelectElement(_driver.FindElementWait(DropDownWithdrawalCountBy)).SelectedOption.Text; }
        }

        public string WithdrawalCountValue
        {
            get { return _driver.FindElement(InputWithdrawalCountBy).GetAttribute("value"); }
        }

        public IWebElement InputWithdrawalCount
        {
            get { return _driver.FindElementWait(InputWithdrawalCountBy); }
        }

        public void SetWithdrawalCountCheck(string criteria, string count)
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(CheckBoxWithdrawalCount);
            _driver.WaitForElementClickable(DropDownWithdrawalCount);
            _driver.ScrollingToBottomofAPage();
            new SelectElement(_driver.FindElementWait(DropDownWithdrawalCountBy)).SelectByText(criteria);
            InputWithdrawalCount.Clear();
            InputWithdrawalCount.SendKeys(count);
        }

        public ViewAutoVerificationConfigurationForm UncheckWithdrawalCount()
        {
            Click(CheckBoxWithdrawalCountBy);
            return Submit();
        }

        #endregion

        #region Deposit Count

        public string DropdownDepositCountSelected
        {
            get { return new SelectElement(_driver.FindElementWait(DropDownDepositCountBy)).SelectedOption.Text; }
        }
        
        public string DepositCountValue
        {
            get { return _driver.FindElement(InputDepositCountBy).GetAttribute("value"); }
        }

        public IWebElement InputDepositCount
        {
            get { return _driver.FindElementWait(InputDepositCountBy); }
        }

        public IWebElement CheckBoxDepositCount
        {
            get { return _driver.FindElementWait(CheckBoxDepositCountBy); }
        }

        public IWebElement DropDownDepositCount
        {
            get { return _driver.FindElementWait(DropDownDepositCountBy); }
        }

        public void SetTotalDepositCountCheck(string criteria, string count)
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(CheckBoxDepositCount);
            _driver.WaitForElementClickable(DropDownDepositCount);
            _driver.ScrollingToBottomofAPage();
            new SelectElement(_driver.FindElementWait(DropDownDepositCountBy)).SelectByText(criteria);
            InputDepositCount.Clear();
            InputDepositCount.SendKeys(Keys.Backspace + count);
        }

        public ViewAutoVerificationConfigurationForm UncheckDepositCount()
        {
            Click(CheckBoxDepositCountBy);
            return Submit();
        }

        #endregion

        #region Account Age

        public IWebElement InputDays
        {
            get { return _driver.FindElementWait(InputDaysBy); }
        }

        public void SetAccountAgeCheck(string criteria, string days)
        {
            _driver.ScrollingToBottomofAPage();
            Click(CheckBoxAccountAgeBy);
            new SelectElement(_driver.FindElementWait(DropdownAccountAgeBy)).SelectByText(criteria);
            InputDays.Clear();
            InputDays.SendKeys(Keys.Backspace + days);
        }

        public string DropdownAccountAgeSelected
        {
            get { return new SelectElement(_driver.FindElementWait(DropdownAccountAgeBy)).SelectedOption.Text; }
        }

        public string InputDaysValue
        {
            get { return _driver.FindElement(InputDaysBy).GetAttribute("value"); }
        }

        public ViewAutoVerificationConfigurationForm UncheckAccountAge()
        {
            Click(CheckBoxAccountAgeBy);
            return Submit();
        }

        #endregion


        #region Has payment level

        public void SetHasPaymentLevel()
        {
            Click(CheckBoxHasPaymentLevelBy);
            Click(AvailableItemBy);
            Click(AssignPaymentLevelBy);
        }

        public void NotSetHasPaymentLevel()
        {
            Click(CheckBoxHasPaymentLevelBy);            
        }

        #endregion

        public ViewAutoVerificationConfigurationForm Submit()
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(SaveButton);
            var page = new ViewAutoVerificationConfigurationForm(_driver);
            page.Initialize();
            return page;
        }

        public EditAutoVerificationConfigurationForm(IWebDriver driver)
            : base(driver)
        {}
    }
}