using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class WithdrawalPage : FrontendPageBase
    {
        public WithdrawalPage(IWebDriver driver) : base(driver) { }
        public string FrozenDescription()
        {
            var frozenDescription = _driver.FindElementWait(By.XPath("//p[@data-i18n='withdrawal.frozen.description']"));
            return frozenDescription.Text;
        }
    }
}
