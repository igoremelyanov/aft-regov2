using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SupportedProductsPage : BackendPageBase
    {
        public SupportedProductsPage(IWebDriver driver) : base(driver) {}

        public ManageProductsPage OpenManageProductsPage()
        {
            var manageButton = _driver.FindElementWait(By.Id("btn-assign-brand-product"));
            manageButton.Click();
            var manageProductsPage = new ManageProductsPage(_driver);
            return manageProductsPage;
        }
    }

    public class ManageProductsPage : BackendPageBase
    {
        public ManageProductsPage(IWebDriver driver) : base(driver) {}

        public ViewAssignedProductsPage AssignProducts(string licensee, string brand, string[] products)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-product-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-product-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-product-brand')]"), brand);

            products.ForEach(x => _driver.SelectFromMultiSelect("assignControl", x));

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new ViewAssignedProductsPage(_driver);
            return submittedForm;
        }

        public ViewAssignedProductsPage UnassignProducts(string licensee, string brand, string[] products)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-product-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-product-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-product-brand')]"), brand);

            products.ForEach(x => _driver.SelectFromMultiSelectUnassign("assignControl", x));

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new ViewAssignedProductsPage(_driver);
            return submittedForm;
        }
    }

    public class ViewAssignedProductsPage : BackendPageBase
    {
        public ViewAssignedProductsPage(IWebDriver driver): base(driver) { }

        public string Confirmation
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-view='brand/product-manager/assign']/div")); }
        }

        public string Licensee
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-bind='with: form.fields.licensee']/p")); }
        }

        public string Brand
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-bind='with: form.fields.brand']/p")); }
        }
        
        public bool IsProductDisplayed(string product)
        {            
            return _driver.FindElement(By.XPath(string.Format("//select[contains(@data-bind, 'options: assignedItems')]/option[text()='{0}']", product))).Displayed;
        }
    }
}