using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentLevelSettingsPage : BackendPageBase
    {
        public PaymentLevelSettingsPage(IWebDriver driver) : base(driver) {}

        public string FilterGrid(string brandName)
        {
            var searchBox = _driver.FindElementWait(By.XPath("//input[contains(@data-bind,'value: $root.search')]"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed); 
            searchBox.Clear();
            searchBox.SendKeys(brandName);
            var searchButton = _driver.FindElementWait(By.CssSelector("#search-button"));
            searchButton.Click();
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", brandName);
        }

        public void SelectPlayer(string playerName)
        {
            var userRecord = string.Format("//td[text() =\"{0}\"]",playerName);
            var firstCell = _driver.FindElementWait(By.XPath(userRecord));
            firstCell.Click();
        }

        public SetPaymentLevelPage OpenSetPaymentLevelPage()
        {
            var setPaymentLevelButton = _driver.FindElementWait(By.XPath("//button[@name='btn-SetPaymentLevel']"));
            setPaymentLevelButton.Click();

            var page = new SetPaymentLevelPage(_driver);
            _driver.WaitForJavaScript();

            return page;
        }
    }
}