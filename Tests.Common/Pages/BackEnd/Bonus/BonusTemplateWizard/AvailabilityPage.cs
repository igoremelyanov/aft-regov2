using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class AvailabilityPage : TemplateWizardPageBase
    {
        public AvailabilityPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[2][@class='active']"));
        }

        public RulesPage Next()
        {
            _driver.ScrollPage(0, 900);
            _nextBtn.Click();

            return new RulesPage(_driver);
        }
    }
}