using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Withdrawal;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class VerificationQueuePage : WithdrawalParentQueue
    {
        internal static readonly By ButtonDocsBy = By.XPath("//button[contains(@name,'docs')]");
        internal static readonly By ButtonInvestigateBy = By.XPath("//button[contains(@name,'investigate')]");
        internal static readonly By ButtonVerifyBy = By.XPath("//button[contains(@name,'verify')]");
        internal static readonly By ButtonUnverifyBy = By.XPath("//button[contains(@name,'unverify')]");

        internal static readonly By WithdrawalRiskProfileCheckResultBy = By.XPath(".//td[contains(@aria-describedby,'RiskProfileCheckResult')]");
 
        public bool CheckIfWithdrawalRequestStatusIsFailed(string username, string fullname)
        {
            SelectRecord(username, fullname);

            var withdrawalStatusXPath = By.XPath("//td[text()='New']");
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

        public string GetWithdrawalRiskProfileCheckResult(IWebElement record)
        {
            return record.FindElement(WithdrawalRiskProfileCheckResultBy).Text;
        }

        public SubmitWithdrawalProcessingForm OpenDocumentsForm(IWebElement record)
        {   
            record.Click();
            Click(ButtonDocsBy);
            return new SubmitWithdrawalProcessingForm(_driver);
        }

        public SubmitWithdrawalProcessingForm OpenVerifyForm(IWebElement record)
        {
            record.Click();
            Click(ButtonVerifyBy);
            return new SubmitWithdrawalProcessingForm(_driver);
        }

        public SubmitWithdrawalProcessingForm OpenUnverifyForm(IWebElement record)
        {
            record.Click();
            Click(ButtonUnverifyBy);
            return new SubmitWithdrawalProcessingForm(_driver);
        }

        public SubmitWithdrawalProcessingForm OpenInvestigateForm(IWebElement record)
        {
            record.Click();
            Click(ButtonInvestigateBy);
            return new SubmitWithdrawalProcessingForm(_driver);
        }

        public VerificationQueuePage(IWebDriver driver)
            : base(driver)
        {
        }
    }

}