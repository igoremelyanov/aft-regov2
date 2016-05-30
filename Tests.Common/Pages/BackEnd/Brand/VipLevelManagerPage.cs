using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Brand;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class VipLevelManagerPage : BackendPageBase
    {
        private static readonly By NewButtonBy = By.XPath("//div[@id='vip-level-home']//button[contains(@data-bind, 'click: openAddTab')]");
        private static readonly By EditButtonBy = By.XPath("//div[@id='vip-level-home']//button[contains(@data-bind, 'click: openEditTab')]");
        private static readonly By ViewVButtonBy = By.XPath("//div[@id='vip-level-home']//button[contains(@data-bind, 'click: openViewTab')]");

        public VipLevelManagerPage(IWebDriver driver)
            : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "vip-level-name-search", "vip-level-name-search-button");
            }
        }

        public NewVipLevelForm OpenNewVipLevelForm()
        {
            _driver.FindElementWait(NewButtonBy).Click();

            return new NewVipLevelForm(_driver);
        }

        public EditVipLevelForm OpenEditVipLevelForm()
        {
            _driver.FindElementWait(EditButtonBy).Click();

            return new EditVipLevelForm(_driver);
        }

        public ViewVipLevelForm OpenViewVipLevelForm()
        {
            _driver.FindElementWait(ViewVButtonBy).Click();

            return new ViewVipLevelForm(_driver);
        }

        public DeactivateVipLevelDialog OpenDeactivateDialog(string vipLevelName, bool associatedWithPlayers = false)
        {
            Grid.SelectRecord(vipLevelName);
            var deactivateButton = _driver.FindElementWait(By.Id("vip-level-deactivate-button"));
            deactivateButton.Click();

            if (associatedWithPlayers)
            {
               _driver.SwitchTo().ActiveElement();
                var yesButton = _driver.FindElementWait(By.XPath("//div[@data-view='vip-manager/confirm-dialog']//button[text()='Yes']"));
                yesButton.Click();
            }
                        
            var dialog = new DeactivateVipLevelDialog(_driver);
            return dialog;
         }
        
        public bool CheckDeactivatedVipLevelStatus(string vipLevelName)
        {
            Grid.FilterGrid(vipLevelName);
            var status = _driver.FindElementValue(By.XPath("//td[@aria-describedby='vip-level-list_Status']"));
            if (status == "Inactive")
            {
                return true;
            }
            return false;
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'text: message')]")); }
        }
    }

    public class DeactivateVipLevelDialog : BackendPageBase
    {

        public DeactivateVipLevelDialog(IWebDriver driver) : base(driver){}

        public VipLevelManagerPage Deactivate()
        {
            var descriptionArea = _driver.FindElementWait(By.XPath("//textarea"));
            descriptionArea.SendKeys("valid vip level");
            var deactivateButton = _driver.FindElementWait(By.XPath("//button[text()='Deactivate']"));
            //_driver.ScrollToElement(deactivateButton);
            Thread.Sleep(2000);
            deactivateButton.Click();
            //Thread.Sleep(2000);
            //deactivateButton.Click();
            var closeButton = _driver.FindElementWait(By.XPath("//button[text()='Close']"));
            closeButton.Click();
            return new VipLevelManagerPage(_driver);
        }

        public VipLevelManagerPage Deactivate(string newVipLevel)
        {
            var listPath = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: newVipLevels')]"));
            var vipLevelList = new SelectElement(listPath);
            vipLevelList.SelectByText(newVipLevel);

            var descriptionArea = _driver.FindElementWait(By.XPath("//textarea[@data-bind='value: remark']"));
            descriptionArea.SendKeys("valid vip level");

            var deactivateButton = _driver.FindElementWait(By.XPath("//button[text()='Deactivate']"));
            deactivateButton.Click();
            return new VipLevelManagerPage(_driver);
        }
    }
}
