using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class NotificationPage : TemplateWizardPageBase
    {
        public NotificationPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[5][@class='active']"));
        }

        public SummaryPage Next()
        {
            _nextBtn.Click();

            return new SummaryPage(_driver);
        }
    }
}