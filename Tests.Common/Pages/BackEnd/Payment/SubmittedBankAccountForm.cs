using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedBankAccountForm : BackendPageBase
    {
        public SubmittedBankAccountForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath = "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
                                        "//div/form[@data-view='payments/bank-accounts/view' and @data-active-view='true']";

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public string LicenseeValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'licenseeName')]"));
            }
        }

        public string BrandNameValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'brandName')]")); 
            }
        }

        public string CurrencyValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'currencyCode')]"));
            }
        }

        public string BankAccountIdValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: Model.bankAccountId')]"));
            }
        }

        public string BankAccountNameValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: Model.bankAccountAccountName')]"));
            }
        }

        public string BankAccountNumberValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: Model.bankAccountNumber')]"));
            }
        }

        public string BankNameValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: Model.bankAccountAccountName')]"));
            }
        }

        public string ProvinceValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: Model.bankAccountProvince')]"));
            }
        }

        public string BranchValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: Model.bankAccountBranch')]"));
            }
        }

        public string BankAccountTypeValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'Model.bankAccountAccountTypeName')]"));
            }
        }

        public string BankAccountSupplierName
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'Model.supplierName')]"));
            }
        }

        public string BankAccountContactNumber
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'Model.contactNumber')]"));
            }
        }

        public string BankAccountUsbCobeValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'Model.usbCode')]"));
            }
        }

        public string PurchasedDate
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'purchasedDate')]"));
            }
        }

        public string UtilizationDate
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'Model.utilizationDate')]"));
            }
        }

        public string ExpirationDate
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'Model.expirationDate')]"));
            }
        }

        public string RemarksValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//*[contains(@data-bind, 'remarks')]"));
            }
        }


        public BankAccountManagerPage SwitchToList()
        {
            var bankAccountstab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Bank Accounts']"));
            bankAccountstab.Click();
            var page = new BankAccountManagerPage(_driver);
            return page;
        }
        
    }
}