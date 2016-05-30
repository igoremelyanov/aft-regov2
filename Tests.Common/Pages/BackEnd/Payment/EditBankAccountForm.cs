using System.IO;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditBankAccountForm : BackendPageBase
    {
        public EditBankAccountForm(IWebDriver driver) : base(driver) {}

        public SubmittedBankAccountForm SubmitForPendingAccount(BankAccountData data, string currency, string bank)
        {
            SelectCurrency(                        
                loadingComplete: By.XPath("//div[@data-view='payments/bank-accounts/edit']//span[text()='Currency']"),
                currencyListSelector: By.XPath("//select[contains(@data-bind, 'options: Model.currencies')]"),
                currencyValue: currency);            

            _bankAccountID.Clear();
            _bankAccountID.SendKeys(data.ID);

            SelectBank(
                loadingComplete: By.XPath("//div[@data-view='payments/bank-accounts/edit']//span[text()='Bank Name']"),
                bankListSelector: By.XPath("//select[contains(@data-bind, 'options: Model.banks')]"),
                bankName: bank);

            _bankAccountNumber.Clear();
            _bankAccountNumber.SendKeys(data.Number);

            _bankAccountName.Clear();
            _bankAccountName.SendKeys(data.Name);

            _bankAccountProvince.Clear();
            _bankAccountProvince.SendKeys(data.Province);

            _bankAccountBranch.Clear();
            _bankAccountBranch.SendKeys(data.Branch);

            new SelectElement(_bankAccountType).SelectByText(data.Type);

            _bankAccountSupplierName.Clear();
            _bankAccountSupplierName.SendKeys(data.SupplierName);

            _bankAccountContactNumber.Clear();
            _bankAccountContactNumber.SendKeys(data.ContactNumber.ToString());

            _bankAccountUsbCode.Clear();
            _bankAccountUsbCode.SendKeys(data.UsbCode);

            var purchasedDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.purchasedDate')]"));
            purchasedDate.Clear();
            purchasedDate.SendKeys("2015/10/25");

            var utilizationDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.utilizationDate')]"));
            utilizationDate.Clear();
            utilizationDate.SendKeys("2016/11/25");

            var expirationDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.expirationDate')]"));
            expirationDate.Clear();
            expirationDate.SendKeys("2016/12/25");

            IdFrontImageInput.SendKeys(TempPlayerIdPath("player-back-id.jpg"));
            _driver.ScrollPage(0, 1000);

            IdBackImageInput.SendKeys(TempPlayerIdPath("player-front-id.jpg"));
            _driver.ScrollPage(0, 1200);

            AtmCardImageInput.SendKeys(TempPlayerIdPath("player-front-id.jpg"));

            _driver.ScrollPage(0, 2200);
            _bankAccountRemarks.Clear();
            _bankAccountRemarks.SendKeys(data.Remarks);
            _saveButton.Click();
            var submittedForm = new SubmittedBankAccountForm(_driver);
            return submittedForm;
        }

        private void SelectCurrency(By loadingComplete, By currencyListSelector, string currencyValue)
        {
            _driver.FindElementWait(loadingComplete);
            if (_driver.FindElements(currencyListSelector).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var currencyList = _driver.FindElementWait(currencyListSelector);
                var currencyField = new SelectElement(currencyList);
                currencyField.SelectByText(currencyValue);
            }
        }

        private void SelectBank(By loadingComplete, By bankListSelector, string bankName)
        {
            _driver.FindElementWait(loadingComplete);
            if (_driver.FindElements(bankListSelector).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var bankList = _driver.FindElementWait(bankListSelector);
                var bankField = new SelectElement(bankList);
                bankField.SelectByText(bankName);
            }
        }

        public SubmittedBankAccountForm SubmitForActivatedAccount(BankAccountData data)
        {           
            new SelectElement(_bankAccountType).SelectByText(data.Type);

            _bankAccountSupplierName.Clear();
            _bankAccountSupplierName.SendKeys(data.SupplierName);
            
            _bankAccountContactNumber.Clear();
            _bankAccountContactNumber.SendKeys(data.ContactNumber.ToString());
            
            _bankAccountUsbCode.Clear();
            _bankAccountUsbCode.SendKeys(data.UsbCode);

            var utilizationDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'Model.utilizationDate')]"));
            utilizationDate.Clear();
            utilizationDate.SendKeys("11/25/2016");

            var expirationDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'Model.expirationDate')]"));
            expirationDate.Clear();
            expirationDate.SendKeys("12/25/2016");

            IdFrontImageInput.SendKeys(TempPlayerIdPath("player-back-id.jpg"));
            _driver.ScrollPage(0, 1000);

            IdBackImageInput.SendKeys(TempPlayerIdPath("player-front-id.jpg"));
            _driver.ScrollPage(0, 1200);

            AtmCardImageInput.SendKeys(TempPlayerIdPath("player-front-id.jpg"));

            _driver.ScrollPage(0, 2200);
            _bankAccountRemarks.Clear();
            _bankAccountRemarks.SendKeys(data.Remarks);
            _saveButton.Click();
            var submittedForm = new SubmittedBankAccountForm(_driver);
            return submittedForm;
        }       
        
        private static string TempPlayerIdPath(string fileName)
        {
            var tempFileName = Path.GetTempFileName();
            var path = Path.ChangeExtension(tempFileName, "jpg");
            var resourceName = "AFT.RegoV2.Tests.Common.Resources." + fileName;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    var array = ms.ToArray();
                    File.WriteAllBytes(path, array);
                }
            }
            return path;
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.bankAccountId')]")]
        private IWebElement _bankAccountID;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.bankAccountNumber')]")]
        private IWebElement _bankAccountNumber;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.bankAccountName')]")]
        private IWebElement _bankAccountName;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.bankAccountProvince')]")]
        private IWebElement _bankAccountProvince;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.bankAccountBranch')]")]
        private IWebElement _bankAccountBranch;

        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'Model.bankAccountAccountType')]")]
        private IWebElement _bankAccountType;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.supplierName')]")]
        private IWebElement _bankAccountSupplierName;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.contactNumber')]")]
        private IWebElement _bankAccountContactNumber;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.usbCode')]")]
        private IWebElement _bankAccountUsbCode;        

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId1']")]
        public IWebElement IdFrontImageInput { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId2']")]
        public IWebElement IdBackImageInput { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId3']")]
        public IWebElement AtmCardImageInput { get; set; }
        
        [FindsBy(How = How.XPath, Using = "//textarea[contains(@data-bind, 'value: Model.remarks')]")]
        private IWebElement _bankAccountRemarks;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: save')]")]
        private IWebElement _saveButton;       

#pragma warning restore 649
    }
}