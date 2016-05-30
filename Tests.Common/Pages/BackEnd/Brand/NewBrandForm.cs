using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewBrandForm : BackendPageBase
    {
        public NewBrandForm(IWebDriver driver) : base(driver) {}

        public const string FormXPath = "//li[contains(@class, 'active') and not(contains(@class, 'inactive'))]";

        public string Title
        {
            get { return _driver.FindElementWait(By.XPath(FormXPath + "//span[text()='New Brand']")).Text; }
        }

        public BrandManagerPage SwitchToBrandsTab()
        {
            var brandsTab = _driver.FindElementWait(By.XPath("//span[text()='Brands']"));
            brandsTab.Click();
            return new BrandManagerPage(_driver);
        }

        public SubmittedBrandForm Submit(
            string brandName, 
            string brandCode, 
            string playerPrefix, 
            string brandType = null, 
            string licensee = null, 
            string playerActivationMethod = null,
            string websiteUrl = null,
            string email = null,
            string smsNumber = null)
        {
            licensee = licensee ?? "Flycow";
            SelectLicensee(By.XPath("//label[contains(@for, 'brand-licensee')]"), By.XPath("//select[contains(@id, 'brand-licensee')]"), licensee);
            
            var brandNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'brand-name') and contains(@data-bind, 'id: nameFieldId()')]"));
            brandNameField.SendKeys(brandName);
            var brandCodeField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'brand-code') and contains(@data-bind, 'id: codeFieldId()')]"));
            brandCodeField.SendKeys(brandCode);
            var brandEmailField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'brand-email') and contains(@data-bind, 'id: emailFieldId()')]"));
            brandEmailField.SendKeys(email ?? TestDataGenerator.GetRandomEmail());
            var brandWebsiteUrlField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'brand-website-url') and contains(@data-bind, 'id: websiteUrlFieldId()')]"));
            brandWebsiteUrlField.SendKeys(websiteUrl ?? TestDataGenerator.GetRandomWebsiteUrl());
            var brandsmsNumberField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'brand-sms-number') and contains(@data-bind, 'id: smsNumberFieldId()')]"));
            brandsmsNumberField.SendKeys(smsNumber ?? TestDataGenerator.GetRandomPhoneNumber().Replace("-", string.Empty));

            var brandTypeField = _driver.FindElementWait(By.XPath("//select[contains(@id, 'brand-type')]"));
            var brandTypeList = new SelectElement(brandTypeField);
            switch (brandType)
            {
                case "Deposit":
                    brandTypeList.SelectByText("Deposit");
                    break;
                case "Credit":
                    brandTypeList.SelectByText("Credit");
                    break;
                case "Integrated":
                    brandTypeList.SelectByText("Integrated");
                    break;
                default:
                    brandTypeList.SelectByText("Deposit");
                    break;
            }
            var playerPrefixField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'brand-player-prefix') and contains(@data-bind, 'id: playerPrefixFieldId()')]"));
            playerPrefixField.SendKeys(playerPrefix);

            if (playerActivationMethod != null)
            {
                var playerActivationMethodField = _driver.FindElementWait(By.XPath("//select[contains(@id, 'brand-player-activation-method')]"));
                var playerActivationMethodList = new SelectElement(playerActivationMethodField);
                playerActivationMethodList.SelectByText(playerActivationMethod);   
            }            

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save' and not(contains(@class, 'inactive'))]"));
            _driver.ScrollPage(0, 800);
            saveButton.Click();
            var form = new SubmittedBrandForm(_driver);
            return form;
        }
    }
}