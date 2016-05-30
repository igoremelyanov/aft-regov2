using System;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class Grid
    {
        protected readonly IWebDriver _driver;
        private readonly string _searchBoxXPath;
        private readonly string _searchButtonXPath;
        private readonly string _searchBoxId;
        private readonly string _searchButtonId;

        public Grid(IWebDriver driver, string searchBoxId, string searchButtonId)
        {
            _driver = driver;
            _searchBoxId = searchBoxId;
            _searchButtonId = searchButtonId;
        }

        public Grid(IWebDriver driver, string gridXPath)
        {
            _driver = driver;
            _searchBoxXPath = gridXPath + "//input[@data-bind='value: $root.search']";
            _searchButtonXPath = gridXPath + "//button[@type='submit']";
        }

        protected By SearchBox
        {
            get { return _searchBoxXPath != null ? By.XPath(_searchBoxXPath) : _searchBoxId != null ? By.Id(_searchBoxId) : null; }
        }

        protected By SearchButton
        {
            get { return _searchButtonXPath != null ? By.XPath(_searchButtonXPath) : _searchButtonId != null ? By.Id(_searchButtonId) : null; }
        }

        public void SelectRecord(string name)
        {
            var userRecord = FilterGrid(name, SearchBox, SearchButton);
            var firstCell = _driver.FindElementWait(By.XPath(userRecord));
            firstCell.Click();
        }

        public string FilterGrid(string name)
        {
            return FilterGrid(name, SearchBox, SearchButton);
        }

        public string FilterGrid(string name, By searchBoxBy, By searchButtonBy)
        {
            WaitHelper.WaitUntil(() => _driver.FindElement(searchBoxBy).Displayed);
            var searchBox = WaitHelper.WaitResult(() => _driver.FindElement(searchBoxBy));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(x => searchBox.Displayed);
            searchBox.Clear();
            //TODO: remove header when defect: AFTREGO-3387 will be fixed
            WaitHelper.WaitUntil(() => _driver.FindElement(searchBoxBy).Displayed);
            searchBox.SendKeys(name);
            if (searchButtonBy == null)
            {
                searchBox.SendKeys(Keys.Return);
            }
            else
            {
                var searchButton = _driver.FindElementWait(searchButtonBy);
                searchButton.Click();
            }
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", name);
        }
    }
}
