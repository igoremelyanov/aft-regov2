using System;
using System.Globalization;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineDepositRequestForm : BackendPageBase
    {
        public OfflineDepositRequestForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]/div/div[@data-view='player-manager/offline-deposit/add-request']";

        public SubmittedOfflineDepositRequestForm Submit(decimal amount)
        {
            //var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            //wait.Until(x => _driver.FindElementWait(By.XPath("//span[text()='Offline Deposit Request']")));
            _driver.FindElementWait(By.XPath("//span[text()='Offline Deposit Request']"));

            Amount.SendKeys(amount.ToString(CultureInfo.InvariantCulture));
            _driver.ScrollPage(0, 800);
            SubmitButton.Click();
            var tab = new SubmittedOfflineDepositRequestForm(_driver);
            tab.Initialize();
            return tab;
        }

        public void TryToSubmit(decimal depositRequestAmount)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(x => _driver.FindElementWait(By.XPath("//span[text()='Offline Deposit Request']")));

            Amount.SendKeys(depositRequestAmount.ToString(CultureInfo.InvariantCulture));
            _driver.ScrollPage(0, 800);
            SubmitButton.Click();
        }

        public string ErrorMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'visible: message')]")); }
        }

        public SubmittedOfflineDepositRequestForm SubmitWithBonus(string bonusName, decimal amount)
        {
            Amount.SendKeys(amount.ToString(CultureInfo.InvariantCulture));
            var bonus = _driver.FindElementWait(By.XPath(string.Format("//span[contains(text(), '{0}')]", bonusName)));
            bonus.Click();
            _driver.ScrollPage(0, 600);
            SubmitButton.Click();
            var tab = new SubmittedOfflineDepositRequestForm(_driver);
            return tab;
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = FormXPath + "//*[contains(@data-bind, 'value: amount')]")]
        public IWebElement Amount { get; private set; }

        [FindsBy(How = How.XPath, Using = FormXPath + "//*[contains(@data-bind, 'options: banks')]/*")]
        public IWebElement Bank { get; private set; }

        [FindsBy(How = How.XPath, Using = FormXPath + "//button[text()= 'Submit']")]
        public IWebElement SubmitButton { get; set; }
#pragma warning restore 649
    }
}