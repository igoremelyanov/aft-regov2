using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class PlayerOverviewPage : FrontendPageBase
    {
        public PlayerOverviewPage(IWebDriver driver) : base(driver) {}

        protected override string GetPageUrl()
        {
            return "en-US/Home/Overview";  //Index page
        }
    }
}