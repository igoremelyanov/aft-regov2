using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    public class BasePage
    {
        protected IWebDriver _driver;

        public virtual String PageTitle()
        {
            return "";
        }

        protected BasePage(IWebDriver driver)
        {
            _driver = driver;
            Inititalise();
            IsLoaded();
        }

        public void Inititalise()
        {
            PageFactory.InitElements(_driver, this);
        }

        private void IsLoaded()
        {
            WaitForAjax();
            Assert.AreEqual(PageTitle(), _driver.Title);       
        }

        public void WaitForAjax(int timeoutSecs = 100, bool throwException = false)
        {
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSecs));
                    wait.Until(d =>
                    {
                        try
                        {
                            return ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return true;
                        }
                    });
                    wait.Until(d =>
                    {
                        try
                        {
                            return (bool)((IJavaScriptExecutor)d).ExecuteScript("return (typeof jQuery == 'undefined') || jQuery.active == 0");

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return true;
                        }
                    });
        }
        

        public void WaitForElementToBeClickable(IWebElement element)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementToBeClickable(element));
        }

        public void ClickElementUsingActions(IWebElement element)
        {
            Actions actions = new Actions(_driver);
            actions.Click(element).Build().Perform();
        }

        public void SimpleClickElement(IWebElement element)
        {
            try
            {
                element.Click();
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
            }

        }

        public void EnterText(IWebElement element, String text)
        {
            WaitForElementToBeClickable(element);
            element.Clear();
            element.SendKeys(text);
            WaitForAjax();
            Assert.AreEqual(text, element.GetAttribute("value"));
        }

        public void ScrollToElement(IWebElement element)
        {
            Actions actions = new Actions(_driver);
            actions.MoveToElement(element).Perform();
        }

        public void CheckElement(IWebElement me)
        {
            if (!me.Selected)
            {
                me.Click();
            }
        }

        public bool IsCheckBoxSelected(IWebElement me)
        {
            if (me.Selected)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public void UnCheckElement(IWebElement me)
        {
            WaitForAjax();
            if (me.Selected)
            {
                me.Click();
            }
        }
    }
}
