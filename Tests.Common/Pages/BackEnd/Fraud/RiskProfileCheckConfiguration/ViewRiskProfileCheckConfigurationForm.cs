using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.RiskProfileCheckConfiguration
{
    public class ViewRiskProfileCheckConfigurationForm : BackendPageBase
    {

        internal static readonly By SuccessAlertBy = By.XPath("//div[contains(@class,'alert-success')]");
        internal static readonly By LicenseeBy = By.XPath("//div[contains(@data-bind,'form.fields.licensee')]/p");
        internal static readonly By BrandBy = By.XPath("//div[contains(@data-bind,'form.fields.brand')]/p");
        internal static readonly By CurrencyBy = By.XPath("//div[contains(@data-bind,'form.fields.currency')]/p");
        internal static readonly By SelectedVipLevelsBy = By.XPath("//div/select[contains(@data-bind, 'form.fields.vipLevels.value')]/option");

        //Account age
        internal static readonly By DropdownAccountAgeSelectedBy = By.XPath("//label[contains(@data-bind,'accountAgeOperatorTitle')]");
        internal static readonly By InputDaysBy = By.XPath("//p[contains(@data-bind,'form.fields.accountAge.value')]");

        //Deposit count
        internal static readonly By DropdownDepositCountSelectedBy = By.XPath("//label[contains(@data-bind,'totalDepositCountOperatorTitle()')]");
        internal static readonly By InputDepositCountBy = By.XPath("//p[contains(@data-bind,'form.fields.totalDepositCountAmount.value')]");

        //Withdrawal count
        internal static readonly By DropdownWithdrawalCountSelectedBy = By.XPath("//label[contains(@data-bind,'totalWithdrawalCountOperatorTitle()')]");
        internal static readonly By InputWithdrawalCountBy = By.XPath("//p[contains(@data-bind,'form.fields.totalWithdrawalCountAmount.value')]");

        //Win loss
        internal static readonly By DropdownWinLossSelectedBy = By.XPath("//label[contains(@data-bind,'winLossOperatorTitle()')]");
        internal static readonly By InputWinLossAmountBy = By.XPath("//p[contains(@data-bind,'form.fields.winLossAmount.value')]");


        #region General fields

        public IWebElement SuccessAlert
        {
            get { return _driver.FindElementWait(SuccessAlertBy); }
        }

        public IWebElement Licensee
        {
            get { return _driver.FindElementWait(LicenseeBy); }
        }

        public IWebElement Brand
        {
            get { return _driver.FindElementWait(BrandBy); }
        }
        
        public IWebElement Currency
        {
            get { return _driver.FindElementWait(CurrencyBy); }
        }

        public IReadOnlyCollection<IWebElement> SelectedVipLevels
        {
            get { return _driver.FindElements(SelectedVipLevelsBy); }
        }

        public List<string> GetSelectedVipLevels()
        {
            return SelectedVipLevels.Select(y => y.Text).ToList();
        }
        #endregion

        #region Account Age

        public IWebElement DropdownAccountAgeSelected
        {
            get { return _driver.FindElementWait(DropdownAccountAgeSelectedBy); }
        }

        public IWebElement InputDays
        {
            get { return _driver.FindElementWait(InputDaysBy); }
        }

        #endregion

        #region Deposit Count

        public IWebElement DropdownDepositCountSelected
        {
            get { return _driver.FindElementWait(DropdownDepositCountSelectedBy); }
        }

        public IWebElement InputDepositCount
        {
            get { return _driver.FindElementWait(InputDepositCountBy); }
        }

        #endregion

        #region Win Loss

        public IWebElement DropdownWinLossSelected
        {
            get { return _driver.FindElementWait(DropdownWinLossSelectedBy); }
        }

        public IWebElement InputWinLossAmount
        {
            get { return _driver.FindElementWait(InputWinLossAmountBy); }
        }

        #endregion

        #region Withdrawal Count

        public IWebElement DropdownWithdrawalCountSelected
        {
            get { return _driver.FindElementWait(DropdownWithdrawalCountSelectedBy); }
        }

        public IWebElement InputWithdrawalCount
        {
            get { return _driver.FindElementWait(InputWithdrawalCountBy); }
        }

        #endregion

        public ViewRiskProfileCheckConfigurationForm(IWebDriver driver)
            : base(driver)
        { }
    }
}
