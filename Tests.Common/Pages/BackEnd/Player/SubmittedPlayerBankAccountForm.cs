using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedPlayerBankAccountForm : BackendPageBase
    {
        public SubmittedPlayerBankAccountForm(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-view='payments/player-bank-accounts/edit']/div")); }
        }

        public string Username
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'username')]"));
            }
        }

        public string BankName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'bank')]/div/p"));
            }
        }

        public string Province
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'province')]/div/p"));
            }
        }

        public string City
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'city')]/div/p"));
            }
        }

        public string Branch
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'branch')]/div/p"));
            }
        }

        public string SwiftCode
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'swiftCode')]/div/p"));
            }
        }

        public string Address
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'address')]/div/p"));
            }
        }

        public string BankAccountName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'accountName')]/div/p"));
            }
        }

        public string BankAccountNumber
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'accountNumber')]/div/p"));
            }
        }

        public string Status
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//*[contains(@data-bind, 'status')]/div/p"));
            }
        }
    }
}