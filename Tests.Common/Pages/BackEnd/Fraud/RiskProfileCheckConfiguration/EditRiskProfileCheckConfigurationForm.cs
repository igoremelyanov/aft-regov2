using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.RiskProfileCheckConfiguration
{
    public class EditRiskProfileCheckConfigurationForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy = By.XPath("//button[contains(@type,'submit')]");
        internal static readonly By ValidationMessageBy = By.XPath("//span[contains(@data-bind,'validationMessage')]");

        internal static readonly By VipLevelBy = By.XPath("//div[contains(@data-bind, 'form.fields.vipLevels.value')]");
        internal static readonly By CurrencyContainerBy = By.Id("risk-check-currency");

        //Account age
        internal static readonly By CheckBoxAccountAgeBy = By.Id("risk-check-has-account-age");
        internal static readonly By DropdownAccountAgeBy = By.XPath("//select[contains(@data-bind,'form.fields.hasAccountAge.value()')]");
        internal static readonly By InputDaysBy = By.XPath("//input[contains(@data-bind,'form.fields.accountAge.value')]");

        //Deposit count
        internal static readonly By CheckBoxDepositCountBy = By.Id("risk-check-has-deposit-count");
        internal static readonly By DropDownDepositCountBy = By.XPath("//select[contains(@data-bind,'form.fields.hasDepositCount.value')]");
        internal static readonly By InputDepositCountBy = By.XPath("//input[contains(@data-bind,'form.fields.totalDepositCountAmount.value')]");

        //Withdrawal count
        internal static readonly By CheckBoxWithdrawalCountBy = By.Id("risk-check-has-withdrawal-count");
        internal static readonly By DropDownWithdrawalCountBy = By.XPath("//select[contains(@data-bind,'form.fields.hasWithdrawalCount.value()')]");
        internal static readonly By InputWithdrawalCountBy = By.XPath("//input[contains(@data-bind,'form.fields.totalWithdrawalCountAmount.value')]");

        //Win Loss
        internal static readonly By CheckBoxWinLossBy = By.Id("risk-check-has-win-loss");
        internal static readonly By DropDownWinLossBy = By.XPath("//select[contains(@data-bind,'form.fields.hasWinLoss.value')]");
        internal static readonly By InputWinLossAmountBy = By.XPath("//input[contains(@data-bind,'form.fields.winLossAmount.value')]");

        public IWebElement ValidationMessage
        {
            get { return _driver.FindElementWait(ValidationMessageBy); }
        }

        public IWebElement SaveButton
        {
            get { return _driver.FindElementWait(SaveButtonBy); }
        }

        public void EditRiskProfileConfigurationFields(RiskProfileCheckConfigurationData data, RiskProfileCheckConfigurationData dataEdited)
        {  
            //Set Currency
            new SelectElement(_driver.FindElementWait(CurrencyContainerBy)).SelectByText(dataEdited.Currency);

            //Set VIP level
            new MultiSelectWidget(_driver, VipLevelBy).DeselectFromMultiSelect(data.VipLevel);
            new MultiSelectWidget(_driver, VipLevelBy).SelectFromMultiSelect(dataEdited.VipLevel);
        }

        public ViewRiskProfileCheckConfigurationForm Submit()
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(SaveButton);
            var page = new ViewRiskProfileCheckConfigurationForm(_driver);
            page.Initialize();
            return page;
        }

        #region Account Age

        public IWebElement InputDays
        {
            get { return _driver.FindElementWait(InputDaysBy); }
        }

        public void SetAccountAgeCheck(string criteria, string days)
        {
            Click(CheckBoxAccountAgeBy);
            new SelectElement(_driver.FindElementWait(DropdownAccountAgeBy)).SelectByText(criteria);
            InputDays.Clear();
            InputDays.SendKeys(days);
        }

        public string DropdownAccountAgeSelected
        {
            get { return new SelectElement(_driver.FindElementWait(DropdownAccountAgeBy)).SelectedOption.Text; }
        }

        public string InputDaysValue
        {
            get { return _driver.FindElement(InputDaysBy).GetAttribute("value"); }
        }

        public ViewRiskProfileCheckConfigurationForm UncheckAccountAge()
        {
            Click(CheckBoxAccountAgeBy);
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

        public void SetDepositCountCheck(string criteria, string count)
        {
            Click(CheckBoxDepositCountBy);
            new SelectElement(_driver.FindElementWait(DropDownDepositCountBy)).SelectByText(criteria);
            InputDepositCount.Clear();
            InputDepositCount.SendKeys(count);
        }

        public ViewRiskProfileCheckConfigurationForm UncheckDepositCount()
        {
            Click(CheckBoxDepositCountBy);
            return Submit();
        }

        #endregion

        #region Withdrawal Count

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
            Click(CheckBoxWithdrawalCountBy);
            new SelectElement(_driver.FindElementWait(DropDownWithdrawalCountBy)).SelectByText(criteria);
            InputWithdrawalCount.Clear();
            InputWithdrawalCount.SendKeys(count);
        }

        public ViewRiskProfileCheckConfigurationForm UncheckWithdrawalCount()
        {
            Click(CheckBoxWithdrawalCountBy);
            return Submit();
        }

        #endregion

        #region WinLoss

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

        public IWebElement CheckBoxWinLoss
        {
            get { return _driver.FindElementWait(CheckBoxWinLossBy); }
        }

        public void SetWinLossCheck(string criteria, string amount)
        {
            Click(CheckBoxWinLossBy);
            new SelectElement(_driver.FindElementWait(DropDownWinLossBy)).SelectByText(criteria);
            InputWinLossAmount.Clear();
            InputWinLossAmount.SendKeys(Keys.Backspace + amount);
        }

        public ViewRiskProfileCheckConfigurationForm UncheckWinLoss()
        {
            _driver.ScrollingToBottomofAPage();
            _driver.WaitAndClickElement(CheckBoxWinLoss);
          
            return Submit();
        }

        #endregion

        public EditRiskProfileCheckConfigurationForm(IWebDriver driver)
            : base(driver)
        { }
    }
}
