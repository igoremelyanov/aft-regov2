using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Withdrawal;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class AcceptanceQueuePage : WithdrawalParentQueue
    {
        public AcceptanceQueuePage(IWebDriver driver) : base(driver) { }

        public bool CheckIfWithdrawalRequestStatusIsVerified(string username, string fullname)
        {
            SelectRecord(username, fullname);

            var withdrawalStatusXPath = By.XPath("//td[text()='Verified']");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(d =>
            {
                var foundElement = _driver.FindElements(withdrawalStatusXPath).FirstOrDefault();
                return foundElement != null;
            });
            return true;
        }

        public void SelectRecord(string username, string fullname)
        {
            var userRecord = FilterGrid(username, fullname);
            var firstCell = _driver.FindElementWait(By.XPath(userRecord));
            firstCell.Click();
        }

        public string FilterGrid(string username, string fullname)
        {
            var searchBox = _driver.FindElementWait(By.XPath("//input[@data-bind='value: $root.search']"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed);
            searchBox.Clear();
            searchBox.SendKeys(username);
            var searchButton = _driver.FindElementWait(By.CssSelector("#search-button"));
            searchButton.Click();
            _driver.WaitForJavaScript();
            return string.Format("//td[text() ='{0}']", fullname);
        }
      
    }
    

}