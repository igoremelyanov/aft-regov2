using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using AFT.RegoV2.Shared.Utils;
using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Extensions
{
    public static class WebDriverExtensions
    {
        public static void WaitForJavaScript(this IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript("return (typeof jQuery == 'undefined') || jQuery.active == 0"));
        }
        
        /// <summary>
        /// Wait for webelement is displayed and enabled
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static bool WaitForElementClickable(this IWebDriver driver, IWebElement element, int seconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
                bool visible = wait.Until(d =>
                {
                    if (element == null)
                        return false;
                    return element.Displayed && element.Enabled;
                });

                return visible;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }


        /// <summary>
        /// Wait for webelement is invisible
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static bool WaitForElementInvisible(this IWebDriver driver, IWebElement element, int seconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
                bool invisible = wait.Until(d =>
                {
                    if (element == null)
                        return true;
                    return !element.Displayed;
                });

                return invisible;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }

        /// <summary>
        /// Clicks on the Element via ExecuteScript function
        /// </summary>
        /// /// <param name="driver"></param>
        /// <param name="element">Element to click</param>
        public static void ClickViaJavaScript(this IWebDriver driver, IWebElement element)
        {
            var javaScriptExecutor = (IJavaScriptExecutor)driver;
            javaScriptExecutor.ExecuteScript("arguments[0].click()", element);
         }

        
        /// <returns>
        /// Find element wrapper. Return  Null if element is not found.
        /// </returns>
        public static IWebElement FindElementSafely(this IWebDriver driver,By selector)
        {
          
            try
            {
                return driver.FindElement(selector);
            }
            catch (NoSuchElementException)
            {
              return null;
            }
        }

        /// <summary>
        /// Wait  on the unstable Element is clickable, then Click 
        /// </summary>
        public static void WaitAndClickElement(this IWebDriver driver, IWebElement element, int timeOut = 10, int sleepInterval = 250)
        {
            driver.WaitForJavaScript();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOut));
            wait.PollingInterval = TimeSpan.FromMilliseconds(sleepInterval);
            bool clicked = wait.Until(d =>
            {
                try
                {
                    if (!element.Displayed || !element.Enabled)
                        return false;

                    element.Click();
                    return true;
                }
                catch (WebDriverException e)
                {
                    if (e.Message.Contains("Element is not clickable"))
                    {
                        return false;
                    }
                    throw;
                }
            });
        }

        /// <summary>
        /// Wait  the field is entered
        /// </summary>
        public static void WaitFieldIsEntered(this IWebDriver driver, IWebElement element, int timeOut = 10,
            int sleepInterval = 250)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOut));
            wait.PollingInterval = TimeSpan.FromMilliseconds(sleepInterval);

            wait.Until(d => !(element.Text.Length > 0));

        }

        /// <summary>
        /// if the page url contains the text
        /// </summary>
        public static bool IfUrlContainsSubstr(this IWebDriver driver, string text)
        {
            return driver.Url.Contains(text);
        }

        public static IWebElement FindElementWait(this IWebDriver driver, By by)
        {
            var elements = FindElementsWait(driver, @by);

            return elements.First();
        }

        public static IWebElement FindLastElementWait(this IWebDriver driver, By by)
        {
            var elements = FindElementsWait(driver, @by);

            return elements.Last();
        }

        public static IEnumerable<IWebElement> FindElementsWait(this IWebDriver driver, By by)
        {
            var elements = FindAnyElementsWait(driver, @by, x => x.Displayed && x.Enabled);

            return elements;
        }

        public static IWebElement FindAnyElementWait(this IWebDriver driver, By by, Func<IWebElement, bool> predicate = null)
        {
            var elements = FindAnyElementsWait(driver, by, predicate);

            return elements.First();
        }

        public static IEnumerable<IWebElement> FindAnyElementsWait(this IWebDriver driver, By by, Func<IWebElement, bool> predicate = null)
        {
            driver.WaitForJavaScript();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            IEnumerable<IWebElement> foundElements = null;

            const int maxAttemptCount = 5;
            var attemptCount = 0;
            while (true)
            {
                try
                {
                    wait.Until(d =>
                    {
                        foundElements = driver.FindElements(@by);

                        if (predicate != null)
                            foundElements = foundElements.Where(predicate);

                        return foundElements.Any();
                    });
                    break;
                }
                catch (StaleElementReferenceException)
                {
                    attemptCount++;
                    if (attemptCount < maxAttemptCount)
                    {
                        continue;
                    }
                    throw;
                }
            }

            return foundElements;
        }

        public static string FindElementValue(this IWebDriver driver, By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            IWebElement element = null;

            wait.Until(d =>
            {
                try
                {
                    element = driver.FindElements(@by).FirstOrDefault(x => x.Displayed);
                }
                catch (StaleElementReferenceException)
                {
                    //there may be some page/control refreshes happening during this time
                    //so it's totally fine to ignore this specific exception if it happens
                } 

                return element != null && element.Text != string.Empty;
            });

            return element.Text;
        }

        public static IWebElement FindElementScroll(this IWebDriver driver, By by)
        {
            var element = FindElementWait(driver, @by);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", element);
            return element;
        }

        public static void ScrollingToBottomofAPage(this IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver)
            .ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
        }

        public static void ScrollingToTopofAPage(this IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver)
            .ExecuteScript("window.scrollTo(document.body.scrollHeight, 0)");
        }

        public static void SelectRecordInGridExtendFilter(this IWebDriver driver, string column, string condition, string value)
        {
            driver.TypeFilterCriterion(column, condition, value);
            driver.GenerateReport();
            var userRecord = String.Format("//td[text() =\"{0}\"]", value);
            var firstCell = driver.FindElementWait(By.XPath(userRecord));
            firstCell.Click();
        }

        public static void OpenExtendedFilter(this IWebDriver driver)
        {
            var showFilterButton = driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: showFilter')]|//i[contains(@data-bind, 'click: showFilter')]"));
            showFilterButton.Click();
        }

        public static void TypeFilterCriterion(this IWebDriver driver, string column, string condition, string value)
        {
            var columnDropDownXPath = By.XPath("//select[contains(@data-bind, 'value: filterField')]");

            if (driver.FindElements(columnDropDownXPath).Count(x => x.Displayed) == 0)
            {
                OpenExtendedFilter(driver);
            }
            var columnDropDown = driver.FindElementWait(columnDropDownXPath);
            var columnField = new SelectElement(columnDropDown);
            columnField.SelectByText(column);

            var conditionDropDown = driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'value: condition')]"));
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(x => conditionDropDown.Displayed);
            var conditionField = new SelectElement(conditionDropDown);
            conditionField.SelectByText(condition);

            var valueField = driver.FindAnyElementWait(By.XPath("//input[@name='value']"));
            if (valueField.Displayed)
            {
                valueField.Clear();
                valueField.SendKeys(value);
            }
            else
            {
                var valueDropDown = driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'value: selectedValue')]"));
                new SelectElement(valueDropDown).SelectByText(value);
            }
        }

        public static void GenerateReport(this IWebDriver driver)
        {
            var searchButton = driver.FindLastElementWait(By.XPath("//button[text()='Search']|//button[@type='submit']"));
            searchButton.Click();
        }

        public static Uri Uri(this IWebDriver driver)
        {
            return new Uri(driver.Url);
        }

        public static void ScrollPage(this IWebDriver driver, int x, int y)
        {
            var coordinatesToScroll = String.Format("scroll('{0}', '{1}')", x, y);
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript(coordinatesToScroll);
        }

        public static void ScrollToElement(this IWebDriver driver, IWebElement element, int shift = -100)
        {
            var scrollScript = string.Format(@"$('html, body').animate({{ scrollTop: ($(arguments[0]).offset().top + {0}) }}, 10);", shift);
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript(scrollScript, element);
        }

        public static void SelectFromMultiSelect(this IWebDriver driver, string controlName, string optionToSelect, bool autoScrollTo = true, bool autoOptionSearch = true)
        {
            var controlPath = String.Format("//div[contains(@data-bind, 'with: {0}')]", controlName);
            var control = driver.FindElementWait(By.XPath(controlPath));

            if (autoScrollTo)
            {
                driver.ScrollToElement(control);
            }

            var list = control.FindElement(By.XPath(".//select[contains(@data-bind, 'options: availableItems')]"));
            var listField = new SelectElement(list);

            if (autoOptionSearch)
            {
                var option = listField.Options.First(x => x.Text == optionToSelect);
                var script = string.Format(@"$(arguments[0]).scrollTop($(arguments[0]).scrollTop() + {0});", option.Location.Y - list.Location.Y);
                ((IJavaScriptExecutor)driver).ExecuteScript(script, list);
            }

            listField.SelectByText(optionToSelect);
            var assignButton = control.FindElements(By.XPath(controlPath + "//button[contains(@data-bind, 'click: assign')]")).Last();
            assignButton.Click();
        }

        public static void SelectFromMultiSelectUnassign(this IWebDriver driver, string controlName, string optionToSelect, bool autoScrollTo = true, bool autoOptionSearch = true)
        {
            var controlPath = String.Format("//div[contains(@data-bind, 'with: {0}')]", controlName);
            var control = driver.FindElementWait(By.XPath(controlPath));

            if (autoScrollTo)
            {
                driver.ScrollToElement(control);
            }

            var list = control.FindElement(By.XPath(".//select[contains(@data-bind, 'options: assignedItems')]"));
            var listField = new SelectElement(list);

            if (autoOptionSearch)
            {
                var option = listField.Options.First(x => x.Text == optionToSelect);
                var script = string.Format(@"$(arguments[0]).scrollTop($(arguments[0]).scrollTop() + {0});", option.Location.Y - list.Location.Y);
                ((IJavaScriptExecutor)driver).ExecuteScript(script, list);
            }

            listField.SelectByText(optionToSelect);
            var unassignButton = control.FindElements(By.XPath(controlPath + "//button[contains(@data-bind, 'click: unassign')]")).Last();
            unassignButton.Click();
        }

        public static void FindElementClick(this IWebDriver driver, string element)
        {
            driver.FindElementWait(By.XPath(element)).Click();
        }

        public static IWebElement UpdateTextField(this IWebDriver driver, By by, string text, bool clearCurrentText = false)
        {
            var textField = driver.FindElementWait(@by);

            if (clearCurrentText) textField.Clear();

            textField.SendKeys(text);

            return textField;
        }

        public static IWebElement UpdateCheckboxState(this IWebDriver driver, By by, bool targetStateIsChecked)
        {
            var checkBoxField = driver.FindElementWait(@by);

            if (checkBoxField.Selected && !targetStateIsChecked || !checkBoxField.Selected && targetStateIsChecked)
                checkBoxField.Click();

            return checkBoxField;
        }

        public static IWebElement GetElementByIdStartsWith(this IEnumerable<IWebElement> elements, string id)
        {
            return elements.GetElementByAttributeStartsWith("id", id);
        }

        public static IWebElement GetElementByAttributeStartsWith(this IEnumerable<IWebElement> elements, string attributeName, string attributeValue)
        {
            return elements.FirstOrDefault(x => x.GetAttribute(attributeName).StartsWith(attributeValue));
        }

        public static string GetFieldValue(this IEnumerable<IWebElement> elements, string fieldName)
        {
            var element = elements.GetElementByAttributeStartsWith("data-bind", "text: fields." + fieldName);
            if (element != null)
            {
                return element.Text;
            }
            return null;
        }

        public static void SaveScreenshot(this IWebDriver driver, string descriptor = "")
        {
            if (driver == null)
                return;

            var path = WebConfigurationManager.AppSettings["ScreenshotsPath"];
            Directory.CreateDirectory(path);
            var testName = TestContext.CurrentContext.Test.Name;
            var cleanedFileName = String.Join(string.Empty, testName.Split(Path.GetInvalidFileNameChars()));
            string cleanedDescriptor = string.IsNullOrEmpty(descriptor) 
                ? string.Empty : 
                "-{0}".Args(String.Join(string.Empty, descriptor.Split(Path.GetInvalidFileNameChars())));

            var fileName = string.Format("{0:yyyy-MM-dd_hh-mm}-{1}{3}.{2}", DateTime.Now, cleanedFileName, "png", cleanedDescriptor);
            var fullPath = Path.Combine(path, fileName);
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (!File.Exists(fullPath))
                {
                    break;
                }
                fileName = string.Format("{0:yyyy-MM-dd_hh-mm}-{1}{3}-{4:D2}.{2}", DateTime.Now, cleanedFileName, "png", cleanedDescriptor, i);
                fullPath = Path.Combine(path, fileName);
            }

            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            Thread.Sleep(500);
            screenshot.SaveAsFile(fullPath, ImageFormat.Png);
            Thread.Sleep(500);
        }

        public static void SaveScreenshotWithVisibleArea(this IWebDriver driver, string descriptor = "")
        {
            if (driver == null)
                return;

            const string markerId = "wdVisibleAreaMarker";

            string markVisibleArea = string.Format(@"
            new function() {{
                var marker = document.createElement(""div"");
                marker.id = '{0}';
                marker.style.backgroundColor = 'green';
                marker.style.width = document.documentElement.clientWidth + 'px';
                marker.style.height = document.documentElement.clientHeight + 'px';
                marker.style.position = 'absolute';
                marker.style.top = document.documentElement.scrollTop + 'px';
                marker.style.left = 0;  
                marker.style.zIndex = 100;  
                marker.style.opacity = 0.2;
                var body = document.getElementsByTagName('body')[0];
                body.appendChild(marker);                      
            }}();", markerId);

            string unmarkVisibleArea = string.Format(@"
            new function() {{
                var marker = document.getElementById('{0}');
                var body = document.getElementsByTagName('body')[0];  
                body.removeChild(marker);                
            }}();", markerId); ;

            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript(markVisibleArea);

            driver.SaveScreenshot(descriptor);

            jse.ExecuteScript(unmarkVisibleArea);
        }
    }
}