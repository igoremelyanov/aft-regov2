using System.IO;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewBankAccountForm : BackendPageBase
    {
        public NewBankAccountForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath = "//li[contains(@class, 'active') and not(contains(@class, 'inactive'))]";
        public string Title
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//span[text()='New Bank Account']")); 
            }
        }

        public SubmittedBankAccountForm Submit(string brandName, string bankAccountId, string bankAccountName, string bankAccountNumber,
            string bankName, string branchProvince)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'bank-account-licensee')]"),
                By.XPath("//select[contains(@id, 'bank-account-licensee')]"), "Flycow", By.XPath("//select[contains(@id, 'bank-account-brand')]"), brandName);

            //ignored until currency list is redesigned
            //var currencyList = _driver.FindElementWait(By.XPath("//select[contains(@id, 'payment-level-currency')]"));
            //var currencyField = new SelectElement(currencyList);
            //currencyField.SelectByText(currency);

            var bankAccountIdField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountId')]"));
            bankAccountIdField.SendKeys(bankAccountId);
            var bankAccountNameField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountName')]"));
            bankAccountNameField.SendKeys(bankAccountName);
            var bankAccountNumberField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountNumber')]"));
            bankAccountNumberField.SendKeys(bankAccountNumber);
            //var bankNameList =
            //    _driver.FindElementWait(By.XPath("//select[contains(@id, 'bank-account-bank')]"));
            //var bankNameField = new SelectElement(bankNameList);
            //bankNameField.SelectByText(bankName);

            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.province')]"));
            provinceField.SendKeys(branchProvince);
            var branchField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.branch')]"));
            branchField.SendKeys(branchProvince);
            _driver.ScrollPage(0, 500);
            var submitButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/add']//button[text()='Save']"));
            submitButton.Click();
            
            var page = new SubmittedBankAccountForm(_driver);
            return page;
        }

        public SubmittedBankAccountForm SubmitWithLicensee(string licensee, string brand, string currency, string bankAccountId, string bankAccountName, string bankAccountNumber,
            string province, string branch, string bankAccountType, string supplierName, string contactNumber, string usbCode)
        {
            SelectLicenseeBrand(By.XPath("//div[@data-view='payments/bank-accounts/add']//span[text()='Licensee']"),
                                By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"), licensee,
                                By.XPath("//select[contains(@data-bind, 'options: Model.brands')]"), brand);

            const string currencyFieldXPath = "//select[contains(@data-bind, 'options: Model.currencies')]";

            if (currency != null && _driver.FindElements(By.XPath(currencyFieldXPath)).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var currencyList = _driver.FindElementScroll(By.XPath(currencyFieldXPath));
                var currencyField = new SelectElement(currencyList);
                currencyField.SelectByText(currency);   
            }

            var bankAccountIdField =
                _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.bankAccountId')]"));
            bankAccountIdField.SendKeys(bankAccountId);

            //var bankNameField =
            //    _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.bankId')]"));
            //bankNameField.SendKeys(bankName);

            var bankAccountNumberField =
                _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.bankAccountNumber')]"));
            bankAccountNumberField.SendKeys(bankAccountNumber);

            var bankAccountNameField =
                _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.bankAccountName')]"));
            bankAccountNameField.SendKeys(bankAccountName);
            
            var provinceField = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.bankAccountProvince')]"));
            provinceField.SendKeys(province);

            var branchField = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.bankAccountBranch')]"));
            branchField.SendKeys(branch);
                        
            var bankaccounttypeList = _driver.FindElementScroll(By.XPath("//select[contains(@data-bind, 'value: Model.bankAccountAccountType')]"));
            var bankaccounttypeField = new SelectElement(bankaccounttypeList);
            bankaccounttypeField.SelectByText(bankAccountType);

            var supplierNameField = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.supplierName')]"));
            supplierNameField.SendKeys(supplierName);

            var contactNumberField = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.contactNumber')]"));
            contactNumberField.SendKeys(contactNumber);

            var usbCodeField = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.usbCode')]"));
            usbCodeField.SendKeys(usbCode);

            var purchasedDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.purchasedDate')]"));           
            purchasedDate.SendKeys("2015/10/24");            

            var utilizationDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.utilizationDate')]"));
            utilizationDate.SendKeys("2016/11/24");

            var expirationDate = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'value: Model.expirationDate')]"));
            expirationDate.SendKeys("2016/12/24");
                        
            _driver.FindElementScroll(By.XPath("//label[contains(@data-bind, 'attr: { for: Model.uploadId1FieldId() }')]"));            
            IdFrontImageInput.SendKeys(TempPlayerIdPath("player-front-id.jpg"));
                        
            _driver.FindElementScroll(By.XPath("//label[contains(@data-bind, 'attr: { for: Model.uploadId2FieldId() }')]"));            
            IdBackImageInput.SendKeys(TempPlayerIdPath("player-back-id.jpg"));
           
            _driver.FindElementScroll(By.XPath("//label[contains(@data-bind, 'attr: { for: Model.uploadId3FieldId() }')]"));            
            AtmCardImageInput.SendKeys(TempPlayerIdPath("player-back-id.jpg"));

            var remarks = _driver.FindElementScroll(By.XPath("//input[contains(@data-bind, 'Model.remarks')]"));
            remarks.SendKeys("new created Bank Account by Selenium Test");
            
            var submitButton = _driver.FindElementScroll(By.XPath("//*[@data-bind='click: save']"));
            submitButton.Click();

            var page = new SubmittedBankAccountForm(_driver);
            return page;
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

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId1']")]
        public IWebElement IdFrontImageInput { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId2']")]
        public IWebElement IdBackImageInput { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId3']")]
        public IWebElement AtmCardImageInput { get; set; }


#pragma warning restore 649



    }
}