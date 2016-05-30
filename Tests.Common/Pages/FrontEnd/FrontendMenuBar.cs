using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages
{
    public class FrontendMenuBar
    {
        #region Locators
        internal static readonly By BalanceDropDownBy = By.XPath("//a[@class='balance drop']");
        internal static readonly By CashierOptionBy = By.XPath("//a[@class='cashier']");
        internal static readonly By SettingsDropDownBy = By.XPath("//div/a[contains(@class, 'settings drop')]");
        internal static readonly By ButtonMyBonusBy = By.XPath("//a[contains(., 'My Bonus')]");
        #endregion

        #region Elements
        public IWebElement BalanceDropDown
        {   
            get { return _driver.FindElementWait(BalanceDropDownBy); }
        }

        public IWebElement CashierOption
        {
            get { return _driver.FindElementWait(CashierOptionBy); }
        }

        public IWebElement SettingsDropDown
        {
            get { return _driver.FindElementWait(SettingsDropDownBy); }
        }

        public IWebElement ButtonMyBonus
        {
            get { return _driver.FindElementWait(ButtonMyBonusBy); }
        }

        #endregion


        protected readonly IWebDriver _driver;
 

        public void OpenBalanceDropDown()
        {   
            _driver.WaitForJavaScript();
            _driver.WaitAndClickElement(BalanceDropDown);
        }

        public void OpenSettingsDropDown()
        {
            _driver.WaitForJavaScript();
            _driver.WaitAndClickElement(SettingsDropDown);
        }

        public CashierPage OpenCashierPage()
        {
             OpenBalanceDropDown();
            _driver.WaitAndClickElement(CashierOption);

            var page = new CashierPage(_driver);
            page.Initialize();
            return page;
        }

        public ClaimBonusPage OpenClaimBonusPage()
        {
            OpenSettingsDropDown();
            _driver.WaitAndClickElement(ButtonMyBonus);

            var page = new ClaimBonusPage(_driver);
            page.Initialize();
            return page;
        }

        public WithdrawalPage ClickWithdrawalDropMenu()
        {
            OpenBalanceDropDown();
            var withdrawal = _driver.FindElementWait(By.XPath("//a[@class='withdrawal']"));
            withdrawal.Click();
            return new WithdrawalPage(_driver);
        }

        public GameListPage ClickPlayGamesMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/GameList']"));
            _driver.ScrollToElement(menuItem, -200);
            Thread.Sleep(1000);
            menuItem.Click();
            return new GameListPage(_driver);
        }
        
        private ClaimBonusPage ClickClaimBonusMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/ClaimBonusReward']"));
            menuItem.Click();
            return new ClaimBonusPage(_driver);
        }

        public ReferFriendsPage ClickReferFriendsMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/ReferAFriend']"));
            menuItem.Click();
            return new ReferFriendsPage(_driver);
        }

        public BalanceDetailsPage ClickBalanceInformationMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/BalanceInformation']"));
            menuItem.Click();
            return new BalanceDetailsPage(_driver);
        }
        
        public TransferFundPage ClickTransferFundSubMenu()
        {
            var menuItem = By.XPath("//a[@href='#fundIn']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            var page = new TransferFundPage(_driver);
            page.Initialize();
            return page;
        }

        public TransferFundPage ClickBalanceDetailsSubMenu()
        {
            var menuItem = By.XPath("//a[@href='#balance']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new TransferFundPage(_driver);
        }

        public ClaimBonusPage ClickClaimBonusSubMenu()
        {
            var menuItem = By.XPath("//a[@href='#claimBonus']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickClaimBonusMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new ClaimBonusPage(_driver);
        }

        public OfflineDepositRequestPage ClickOfflineDepositSubmenu()
        {
            var menuItem = By.XPath("//a[@href='#offlineDeposit']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new OfflineDepositRequestPage(_driver);
        }

        public OnlineDepositRequestPage ClickOnlineDepositSubmenu()
        {
            var menuItem = By.XPath("//a[@href='#onlineDeposit']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new OnlineDepositRequestPage(_driver);
        }

        public FrontendMenuBar(IWebDriver driver)
        {
            _driver = driver;
        }
    }
}