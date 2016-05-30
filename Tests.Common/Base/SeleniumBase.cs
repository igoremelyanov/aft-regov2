using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace AFT.RegoV2.Tests.Common.Base
{

    public enum BrowserType
    {
        IE,
        Firefox,
        Chrome
    }

    [Category("Selenium")]
    public abstract class SeleniumBase : TestsBase
    {
        protected IWebDriver _driver;
        protected IUnityContainer _container;
        private DateTimeOffset _startTime;
        
        protected ICommonSettingsProvider SettingsProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _startTime = DateTime.UtcNow;
        }

        public override void BeforeAll()
        {
            base.BeforeAll();

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-CA");
            
            _container = new ApplicationContainerFactory().CreateWithRegisteredTypes();

            SettingsProvider = _container.Resolve<ICommonSettingsProvider>();
            _container.Resolve<SecurityTestHelper>().SignInClaimsSuperAdmin();

            _driver = CreateFireFoxWebDriver();
            _driver.Url = GetWebsiteUrl();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
            _driver.Manage().Window.Size = new Size(1500, 900);
        }

        static IWebDriver CreateChromeWebDriver()
        {
            //use chromium if it's found in predefined folder
            var options = new ChromeOptions();
            const string chromiumFilePath = @"C:\chromium\chrome.exe";
            if (File.Exists(chromiumFilePath))
            {
                options.BinaryLocation = chromiumFilePath;
                const string chromeDriverDirectory = @"C:\chromium";
                return new ChromeDriver(chromeDriverDirectory, options);
            }
            return new ChromeDriver(options);
        }

        static IWebDriver CreateFireFoxWebDriver()
        {
            return new FirefoxDriver();
        }

        private static IWebDriver CreateDriverInstance(BrowserType browserType)
        {
            DesiredCapabilities desiredCapabilities = new DesiredCapabilities();
            switch (browserType)
            {
                case BrowserType.Firefox:
                    {
                        desiredCapabilities = DesiredCapabilities.Firefox();
                        break;
                    }
                case BrowserType.Chrome:
                    {
                        desiredCapabilities = DesiredCapabilities.Chrome();
                        break;
                    }
                case BrowserType.IE:
                    {
                        desiredCapabilities = DesiredCapabilities.InternetExplorer();
                        break;
                    }
            }
            return new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), desiredCapabilities);
        }

        public override void BeforeAllFailed(Exception ex)
        {
            try
            {
                base.BeforeAllFailed(ex);
                SaveScreenshot();
            }
            finally
            {
                QuitWebDriver();
            }
        }

        public override void BeforeEachFailed(Exception ex)
        {
            base.BeforeEachFailed(ex);
            SaveScreenshot();
        }

        public override void AfterEachFailed()
        {
            base.AfterEachFailed();
            SaveScreenshot();
        }

        public override void AfterEach()
        {
            base.AfterEach();

            var eventRepository = _container.Resolve<IEventRepository>();
            var errorsCount = eventRepository.Events.Count(e => (e.DataType == "ErrorRaised" && e.Created > _startTime));
            if (errorsCount > 0)
                Console.WriteLine(@"##teamcity[buildProblem description='" + errorsCount + @" error(s) logged in Events db table after " + TestContext.CurrentContext.Test.Name + @" test!']");
        }

        public override void AfterAll()
        {
            try
            {
                base.AfterAll();
            }
            finally
            {
                QuitWebDriver();
            }
        }

        public void SaveScreenshot()
        {
            _driver.SaveScreenshotWithVisibleArea();
        }

        protected abstract string GetWebsiteUrl();

        protected void QuitWebDriver()
        {
            if (_driver == null) return;

            var exceptionThrown = false;
            var retries = 0;
            do
            {
                try
                {
                    retries++;
                    _driver.Quit();
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                    SaveScreenshot();
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }

            }
            while (exceptionThrown && retries <= 3);
        }
    }


    public class CategorySmoke : CategoryAttribute
    {
        public CategorySmoke() : base("Smoke") { }
    }
}
