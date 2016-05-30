using System;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Configuration;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Tests
{
    class BaseTest
    {
        protected static IWebDriver _driver;
        protected static string memberSiteUrl = WebConfigurationManager.AppSettings["MemberWebsiteUrl"];

        public enum WebDriverType
        {
            Chrome,
            Firefox,
        }

        [SetUp]
        public void Setup()
        {
            _driver = CreateDriver(WebDriverType.Firefox);
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));
            _driver.Navigate().GoToUrl(memberSiteUrl);
        }

        [TearDown]
        public void TearDown()
        {
            //if (ScenarioContext.Current.TestError != null)
            //{
            //    MakeScreenShot();
            //}
            _driver.Quit();
        }

        private static void MakeScreenShot()
        {
            string fileNameBase = string.Format("error_{0}", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            var artifactDirectory = Directory.GetCurrentDirectory();

            ITakesScreenshot takesScreenshot = _driver as ITakesScreenshot;
            var screenshot = takesScreenshot.GetScreenshot();
            string screenshotFilePath = Path.Combine(artifactDirectory, fileNameBase + "_screenshot.png");
            screenshot.SaveAsFile(@fileNameBase + "_screenshot.png", ImageFormat.Png);
            Console.WriteLine("Screenshot: {0}", new Uri(screenshotFilePath));
        }

        public static IWebDriver CreateDriver(WebDriverType browser)
        {
            switch (browser)
            {
                case WebDriverType.Firefox:
                    return CreateFirefoxDriver();
                case WebDriverType.Chrome:
                    return CreateChromeDriver();
                default:
                    throw new NotSupportedException(string.Format("{0} is not supported.", browser));
            }
        }

        private static FirefoxDriver CreateFirefoxDriver()
        {
            return new FirefoxDriver();
        }


        private static ChromeDriver CreateChromeDriver()
        {
            return new ChromeDriver();
        }
    }
}
