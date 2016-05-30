using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class WageringPage: TemplateWizardPageBase
    {
        public WageringPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[4][@class='active']"));
        }

        public NotificationPage Next()
        {
            _nextBtn.Click();

            return new NotificationPage(_driver);
        }

        public SummaryPage NavigateToSummary()
        {
            return Next().Next();
        }
    }
}