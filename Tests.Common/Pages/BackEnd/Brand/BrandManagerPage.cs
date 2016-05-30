using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BrandManagerPage : BackendPageBase
    {
        public const string GridXPath = "//div[contains(@data-view, 'brand/brand-manager/list')]";

        public BrandManagerPage(IWebDriver driver) : base(driver)
        {
        }

        public static By NewButton = By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='New']");
        public static By EditButton = By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='Edit']");
        public static By ActivateButton = By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='Activate']");

        public Grid Grid
        {
            get { return new Grid(_driver, GridXPath); }
        }
        
        public string Title
        {
            get { return _driver.FindElementWait(By.XPath("//h5[text()='Brand Manager']")).Text; } 
        }

        public NewBrandForm OpenNewBrandForm()
        {
            var newButton = FindActionButton("new", GridXPath);
            newButton.Click();
            var newForm = new NewBrandForm(_driver);
            return newForm;
        }

        public bool HasActiveStatus(string brandName)
        {
            Grid.SelectRecord(brandName);
            return _driver.FindElementWait(By.XPath("//div[@id='brand-grid']//td[@title='Active']")).Displayed;
        }

        public bool CheckDeactivatedBrandStatus(string brandName)
        {
            Grid.FilterGrid(brandName);
            var status = _driver.FindElementValue(By.XPath("//td[contains(@aria-describedby,'Status')]"));
            if (status == "Deactivated")
            {
                return true;
            }
            return false;
        }

        public BrandActivateDialog OpenBrandActivateDialog(string brand)
        {
            Grid.SelectRecord(brand);
            var activateButton = FindActionButton("activate", GridXPath);
            activateButton.Click();
            var activateDialog = new BrandActivateDialog(_driver);
            return activateDialog;
        }

        public BrandActivateDialog OpenBrandDeactivateDialog(string brand)
        {
            Grid.SelectRecord(brand);
            var deactivateButton = FindActionButton("deactivate", GridXPath);
            deactivateButton.Click();
            var deactivateDialog = new BrandActivateDialog(_driver);
            return deactivateDialog;
        }

        public EditBrandForm OpenEditBrandForm(string brand)
        {
            Grid.SelectRecord(brand);
            var editButton = FindActionButton("edit", GridXPath);
            editButton.Click();
            var editForm = new EditBrandForm(_driver);
            return editForm;
        }

        public ViewBrandForm OpenViewBrandForm(string brand)
        {
            Grid.SelectRecord(brand);
            var viewButton = FindActionButton("view", GridXPath);
            viewButton.Click();
            var viewForm = new ViewBrandForm(_driver);
            return viewForm;
        }
    }
}