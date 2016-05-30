using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class HeaderMenuBar
    {
        private static string HeaderXpath { get; } = "//section[contains(@class, 'site-header')]";
        private static By Settings { get; } = By.XPath(HeaderXpath + "//a[@href='#settings']");
        private static By MyAccount { get; } = By.XPath(HeaderXpath + "//a[@href='/en-US/Home/PlayerProfile']");

        protected readonly IWebDriver Driver;

        public HeaderMenuBar(IWebDriver driver)
        {
            Driver = driver;
        }

        public PlayerProfilePage OpenMyAccount()
        {
            ScrollToTop();
            var settings = Driver.FindElementWait(Settings);
            settings.Click();
            var myAccount = Driver.FindElementWait(MyAccount);
            myAccount.Click();
            return new PlayerProfilePage(Driver);
        }

        private void ScrollToTop()
        {
            Driver.ScrollPage(0, 0);
        }
    }
}