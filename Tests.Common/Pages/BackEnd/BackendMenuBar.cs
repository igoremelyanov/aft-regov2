using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.FraudManager;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.RiskProfileCheckConfiguration;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Reports;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Withdrawal;
using AFT.RegoV2.Tests.Tests.Selenium.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BackendMenuBar
    {
        protected readonly IWebDriver _driver;
        public static readonly By OfflineDepositConfirm = By.XPath("//span[text()='Offline Deposit Confirm']");
        public static readonly By LanguageManager = By.XPath("//span[text()='Language Manager']");
        public static By BonusTemplateManager = By.XPath("//span[text()='Bonus Template Manager']");
        public static By OfflineWithdrawRequests = By.XPath("//span[text()='Offline Withdraw Requests']");
        public static readonly By BrandManager = By.XPath("//span[text()='Brand Manager']");

        public BackendMenuBar(IWebDriver driver)
        {
            _driver = driver;
        }

        public virtual void Initialize()
        {
            PageFactory.InitElements(_driver, this);
        }


        #region Main Menu

        private IWebElement GetMenuItem(string title)
        {
            string xpath = string.Format("//div[@id='sidebar']//span[@class='menu-text' and text()='{0}']", title);
            return _driver.FindElementWait(By.XPath(xpath));
        }

        //In R1.0 we will not have dashboard and home menu
        //public IWebElement GetHomeMenu
        //{
        //    get { return GetMenuItem("Home"); }
        //}

        public IWebElement GetAdminMenu
        {
            get { return GetMenuItem("Admin"); }
        }

        public IWebElement GetPlayerMenu
        {
            get { return GetMenuItem("Player"); }
        }

        public IWebElement GetWalletMenu
        {
            get { return GetMenuItem("Wallet"); }
        }

        public IWebElement GetPaymentMenu
        {
            get { return GetMenuItem("Payment"); }
        }

        public IWebElement GetWithdrawalMenu
        {
            get { return GetMenuItem("Withdrawal"); }
        }

        public IWebElement GetBonusMenu
        {
            get { return GetMenuItem("Bonus"); }
        }

        public IWebElement GetReportMenu
        {
            get { return GetMenuItem("Report"); }
        }

        public IWebElement GetFraudMenu
        {
            get { return GetMenuItem("Fraud"); }
        }

        public IWebElement GetBrandMenu
        {
            get { return GetMenuItem("Brand"); }
        }

        public IWebElement GetLicenseeMenu
        {
            get { return GetMenuItem("Licensee"); }
        }

        public IWebElement GetProductMenu
        {
            get { return GetMenuItem("Product"); }
        }

        #endregion


        private IWebElement GetPlayerDepositApproveMenuItem
        {
            get { return _driver.FindElementWait(By.XPath("//span[text()='Player Deposit Approve']")); }
        }

        public IWebElement GetOfflineDepositConfirmMenuItem
        {
            get { return _driver.FindElementWait(By.XPath("//span[text()='Offline Deposit Confirm']")); }
        }

        public IWebElement GetPlayerDepositVerifyMenuItem
        {
            get { return _driver.FindElementWait(By.XPath("//div[@id='sidebar']//span[text()='Player Deposit Verify']")); }
        }

        public IWebElement GetCurrencyManagerMenu
        {
            get { return _driver.FindElementWait(By.XPath("//span[text()='Currency Manager']")); }
        }

        public IWebElement GetBrandCurrencyManagerMenu
        {
            get { return _driver.FindElementWait(By.XPath("//span[text()='Brand Currency Manager']")); }
        }

        public RoleManagerPage ClickRoleManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Role Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetAdminMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new RoleManagerPage(_driver);
            return page;
        }

        public IpRegulationManagerPage ClickIpRegulationManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='IP Regulation Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetAdminMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            return new IpRegulationManagerPage(_driver);
        }

        public PlayerManagerPage ClickPlayerManagerMenuItem()
        {
            _driver.WaitForJavaScript();
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Player Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                _driver.WaitAndClickElement(GetPlayerMenu);
            }
            var submenu = _driver.FindElementWait(menuItem);
            _driver.WaitAndClickElement(submenu);

            var page = new PlayerManagerPage(_driver);
            page.Initialize();
            return page;
        }

        public PaymentLevelSettingsPage ClickPaymentLevelSettingsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Payment Level Settings']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                _driver.WaitAndClickElement(GetPlayerMenu);
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new PaymentLevelSettingsPage(_driver);
            return page;
        }

        public PlayerDepositVerifyPage ClickPlayerDepositVerifyItem()
        {
            var menuItem = By.XPath("//span[text()='Player Deposit Verify']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            _driver.ScrollPage(800, 0);
            GetPlayerDepositVerifyMenuItem.Click();
            var page = new PlayerDepositVerifyPage(_driver);
            return page;
        }

        public OfflineDepositConfirmPage ClickOfflineDepositConfirmMenuItem()
        {
            var menuItem = By.XPath("//span[text()='Offline Deposit Confirm']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            _driver.ScrollPage(800, 0);
            GetOfflineDepositConfirmMenuItem.Click();
            var page = new OfflineDepositConfirmPage(_driver);
            page.Initialize();
            return page;
        }

        public PlayerDepositApprovePage ClickPlayerDepositApproveItem()
        {
            var menuItem = By.XPath("//span[text()='Player Deposit Approve']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            _driver.ScrollPage(800, 0);
            _driver.FindElementWait(By.XPath("//span[text()='Player Deposit Approve']"));
            GetPlayerDepositApproveMenuItem.Click();
            var page = new PlayerDepositApprovePage(_driver);
            page.Initialize();
            return page;
        }

        public BrandManagerPage ClickBrandManagerItem()
        {
            var menuItem = By.XPath("//span[text()='Brand Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetBrandMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BrandManagerPage(_driver);
            return page;
        }

        public BankManagerPage ClickBanksItem()
        {
            var menuItem = By.XPath("//span[text()='Banks']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BankManagerPage(_driver);
            return page;
        }

        public BankAccountManagerPage ClickBankAccountsItem()
        {
            var menuItem = By.XPath("//span[text()='Bank Accounts']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BankAccountManagerPage(_driver);
            return page;
        }

        public SupportedCurrenciesPage ClickSupportedCurrenciesMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Supported Currencies']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetBrandMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new SupportedCurrenciesPage(_driver);
            return page;
        }

        public PaymentLevelsPage ClickPaymentLevelsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Payment Level Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new PaymentLevelsPage(_driver);
            return page;
        }

        public PlayerBankAccountVerifyPage ClickPlayerBankAccountVerifyMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Player Bank Account Verify']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new PlayerBankAccountVerifyPage(_driver);
            return page;
        }

        public BonusTemplateManagerPage ClickBonusTemplateMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Bonus template manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                //GetHomeMenu.Click();
                GetBonusMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BonusTemplateManagerPage(_driver);
            return page;
        }

        public BonusManagerPage ClickBonusMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Bonus manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                //GetHomeMenu.Click();
                GetBonusMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BonusManagerPage(_driver);
            return page;
        }

        public WalletManagerPage ClickWalletManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Wallet Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetWalletMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new WalletManagerPage(_driver);
            return page;
        }

        public LicenseeManagerPage ClickLicenseeManagerItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Licensee Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetLicenseeMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new LicenseeManagerPage(_driver);
            return page;
        }

        public SupportedCountriesPage ClickSupportedCountriesMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Supported Countries']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetBrandMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new SupportedCountriesPage(_driver);
            return page;
        }

        public AdminManagerPage ClickAdminManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Admin Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetAdminMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new AdminManagerPage(_driver);
            return page;
        }

        public SupportedLanguagesPage ClickSupportedLanguagesMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Supported Languages']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetBrandMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new SupportedLanguagesPage(_driver);
            return page;
        }

        public LanguageManagerPage ClickLanguageManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//ul[@data-bind='foreach: submenu']//span[text()='Language Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetAdminMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new LanguageManagerPage(_driver);
            return page;
        }

        public PaymentSettingsPage ClickPaymentSettingsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//ul[@data-bind='foreach: submenu']//span[text()='Payment Settings']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new PaymentSettingsPage(_driver);
            return page;
        }

        public TransferSettingsPage ClickTransferSettingsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//ul[@data-bind='foreach: submenu']//span[text()='Transfer Settings']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new TransferSettingsPage(_driver);
            return page;
        }

        public VipLevelManagerPage ClickVipLevelManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//ul[@data-bind='foreach: submenu']//span[text()='VIP Level Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetBrandMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new VipLevelManagerPage(_driver);
            return page;
        }

        public PlayerReportsPage ClickPlayerReportsMenuItem()
        {
            ClickReportsSubmenuItem("Player Reports");
            var page = new PlayerReportsPage(_driver);
            page.Initialize();
            return page;
        }

        public PaymentReportsPage ClickPaymentReportsMenuItem()
        {
            ClickReportsSubmenuItem("Payment Reports");
            var page = new PaymentReportsPage(_driver);
            page.Initialize();
            return page;
        }

        public BrandReportsPage ClickBrandReportsMenuItem()
        {
            ClickReportsSubmenuItem("Brand Reports");
            var page = new BrandReportsPage(_driver);
            page.Initialize();
            return page;
        }

        public void ClickReportsSubmenuItem(string title)
        {
            var menuItem = By.XPath(String.Format("//div[@id='sidebar']//span[text()='{0}']", title));
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetReportMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
        }

        public OfflineWithrawRequestsPage ClickOfflineWithdrawRequestsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Offline Withdraw Requests']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new OfflineWithrawRequestsPage(_driver);
            page.Initialize();
            return page;
        }

        public bool IsOfflineWithdrawRequestsMenuItemPresent()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Offline Withdraw Requests']");
            IList<IWebElement> elems = _driver.FindElements(menuItem);
            return elems.Any();
        }

        public VerificationQueuePage ClickVerificationQueueMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Verification Queue']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetWithdrawalMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new VerificationQueuePage(_driver);
            page.Initialize();
            return page;
        }

        public OnHoldQueuePage ClickOnHoldQueueMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='On Hold Queue']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetWithdrawalMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new OnHoldQueuePage(_driver);
            page.Initialize();
            return page;
        }
     
        
        public AcceptanceQueuePage ClickAcceptanceQueueMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Acceptance Queue']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetWithdrawalMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new AcceptanceQueuePage(_driver);
            page.Initialize();
            return page;
        }

        public OfflineWithrawalApprovalPage ClickOfflineWithdrawalApprovalMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Offline Withdrawal Approval']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new OfflineWithrawalApprovalPage(_driver);
            page.Initialize();
            return page;
        }

        public ProductManagerPage ClickProductManagerMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Product Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetProductMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new ProductManagerPage(_driver);
            return page;
        }

        public bool CheckIfMenuItemDisplayed(By menuItem)
        {
            var element = _driver.FindElements(menuItem).FirstOrDefault();
            if (element == null)
            {
                return false;
            }
            return element.Displayed;
        }

        public SupportedProductsPage ClickSupportedProductsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Supported Products']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetBrandMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new SupportedProductsPage(_driver);
            return page;
        }

        public BetLevelsPage ClickBetLevelsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Bet Limits']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetProductMenu.Click();
            }

            Thread.Sleep(1000);// to slow down webdriver here  
            _driver.ScrollPage(0, 800);

            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BetLevelsPage(_driver);
            return page;
        }

        public BackendIpRegulationsPage ClickBackendIpRegulationsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Backend IP regulations']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetAdminMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BackendIpRegulationsPage(_driver);
            page.Initialize();
            return page;
        }

        public BrandIpRegulationsPage ClickBrandIpRegulationsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Brand IP regulations']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetAdminMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new BrandIpRegulationsPage(_driver);
            page.Initialize();
            return page;
        }

        public PaymentGatewaySettingsPage ClickPaymentGatewaySettingsMenuItem()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//ul[@data-bind='foreach: submenu']//span[text()='Payment Gateway Settings']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                GetPaymentMenu.Click();
            }
            var submenu = _driver.FindElementWait(menuItem);
            submenu.Click();
            var page = new PaymentGatewaySettingsPage(_driver);
            return page;
        }
        #region Fraud Menu Items
        
       
        public FraudRiskLevelPage OpenFraudManager()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Fraud Manager']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                _driver.WaitAndClickElement(GetFraudMenu);
            }
            var submenu = _driver.FindElementWait(menuItem);
            _driver.WaitAndClickElement(submenu);
            var page = new FraudRiskLevelPage(_driver);
            page.Initialize();
            return page;
        }
  

        public AutoVerificationConfigurationPage ClickAutoVerificationConfiguration()
        {
            _driver.WaitForJavaScript();
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Auto Verification Configuration']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                _driver.WaitAndClickElement(GetFraudMenu);
            }
            var submenu = _driver.FindElementWait(menuItem);
            _driver.WaitAndClickElement(submenu);
            
            var page = new AutoVerificationConfigurationPage(_driver);
            page.Initialize();
            return page;
        } 

 
       public RiskProfileCheckConfigurationPage ClickRiskProfileCheckConfiguration()
        {
            var menuItem = By.XPath("//div[@id='sidebar']//span[text()='Risk Profile Check Configuration']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                _driver.WaitAndClickElement(GetFraudMenu);
            }
            var submenu = _driver.FindElementWait(menuItem);
            _driver.WaitAndClickElement(submenu);

            var page = new RiskProfileCheckConfigurationPage(_driver);
            page.Initialize();
            return page;
        }

        #endregion
    }
}

