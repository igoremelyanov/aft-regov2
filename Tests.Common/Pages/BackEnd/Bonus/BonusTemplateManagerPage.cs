using System;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    public class BonusTemplateManagerPage : BackendPageBase
    {
        public BonusTemplateManagerPage(IWebDriver driver) : base(driver)
        {
            Initialize();
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "template-name-search", "templates-search-button");
            }
        }

        public InfoPage OpenNewBonusTemplateForm()
        {
            _newButton.Click();
            return new InfoPage(_driver);
        }


        public InfoPage OpenEditForm(string bonusTemplateName)
        {
            Grid.SelectRecord(bonusTemplateName);
            _editButton.Click();
            return new InfoPage(_driver);
        }

        public SummaryPage OpenViewForm(string bonusTemplateName)
        {
            Grid.SelectRecord(bonusTemplateName);
            _viewButton.Click();
            return new SummaryPage(_driver);
        }

        public DeleteBonusTemplateDialog OpenDeleteBonusTemplateDialog(string bonusTemplateName)
        {
            Grid.SelectRecord(bonusTemplateName);
            _deleteButton.Click();
            return new DeleteBonusTemplateDialog(_driver);
        }

        public bool SearchForDeletedRecord(string bonusTemplateName)
        {
            _searchBox.Clear();
            Thread.Sleep(5000);
            _searchBox.SendKeys(bonusTemplateName);
            Thread.Sleep(5000);
            _searchButon.Click();

            Thread.Sleep(5000); //wait for Record will disappeared from grid. 

            var recordXPath = string.Format("//td[text() =\"{0}\"]", bonusTemplateName);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try
            {
                wait.Until(d =>
                {
                    var foundElements = _driver.FindElements(By.XPath(recordXPath)).FirstOrDefault(x => x.Displayed);
                    return foundElements != null;
                });
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//div[@id='bonus-template-grid']//button[contains(@data-bind, 'click: openAddTemplateTab')]")]
        public IWebElement _newButton { get; private set; }
        [FindsBy(How = How.XPath, Using = "//div[@id='bonus-template-grid']//button[contains(@data-bind, 'click: openEditTemplateTab')]")]
        public IWebElement _editButton { get; private set; }
        [FindsBy(How = How.XPath, Using = "//div[@id='bonus-template-grid']//button[contains(@data-bind, 'click: deleteTemplate')]")]
        public IWebElement _deleteButton { get; private set; }
        [FindsBy(How = How.XPath, Using = "//div[@id='bonus-template-grid']//button[contains(@data-bind, 'click: openViewTemplateTab')]")]
        private IWebElement _viewButton;
        [FindsBy(How = How.XPath, Using = "//div[@id='bonus-template-grid']//input[@id='template-name-search']")]
        public IWebElement _searchBox { get; private set; }
        [FindsBy(How = How.XPath, Using = "//div[@id='bonus-template-grid']//button[@id='templates-search-button']")]
        public IWebElement _searchButon { get; private set; }
#pragma warning restore 649
    }
}