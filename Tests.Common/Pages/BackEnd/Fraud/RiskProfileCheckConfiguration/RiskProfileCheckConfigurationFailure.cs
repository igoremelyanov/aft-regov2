using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.RiskProfileCheckConfiguration
{
    public class RiskProfileCheckConfigurationFailure : BackendPageBase
    {
        internal static readonly By ErrorAlertBy = By.ClassName("alert-danger");


        public IWebElement ErrorAlert
        {
            get { return _driver.FindElementWait(ErrorAlertBy); }
        }


        public RiskProfileCheckConfigurationFailure(IWebDriver driver)
            : base(driver)
        { }
    }
}
