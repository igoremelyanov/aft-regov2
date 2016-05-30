using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewOfflineWithdrawRequest : BackendPageBase
    {
        public ViewOfflineWithdrawRequest(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-view='payments/withdrawal/request']/div")); }
        }
    }
}