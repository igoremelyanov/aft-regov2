using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.RiskProfileCheckConfiguration
{
    /// Represents Fraud -> Risk Profile Check Configuration -> Risk Profile Check Configuration Manager
    public class RiskProfileCheckConfigurationPage : BackendPageBase
    {
        internal static readonly By NewButtonBy = By.XPath("//button[contains(@name,'new')]");
        internal static readonly By EditButtonBy = By.XPath("//button[contains(@name,'edit')]");
        internal static readonly By ScrolGridDropdown = By.XPath("//select[contains(@class,'ui-pg-selbox')]");

        public NewRiskProfileCheckConfigurationForm OpenNewRiskProfileCheckConfigurationForm()
        {
            Click(NewButtonBy);
            var page = new NewRiskProfileCheckConfigurationForm(_driver);
            page.Initialize();
            return page;
        }

        public void SelectRPCRecord(RiskProfileCheckConfigurationData rpcData)
        {
            new SelectElement(_driver.FindElementWait(ScrolGridDropdown)).SelectByText("100");

            _driver.WaitForJavaScript();
            var recordXPath =
                string.Format(
                    "//table//tr[contains(., '{0}') and contains(., '{1}') and contains(., '{2}') and contains(., '{3}')]",
                    rpcData.Licensee, rpcData.Brand, rpcData.Currency, rpcData.VipLevel);

            Click(By.XPath(recordXPath));
        }

        public EditRiskProfileCheckConfigurationForm OpenEditRiskProfileCheckConfigurationForm(RiskProfileCheckConfigurationData rpcData)
        {
            SelectRPCRecord(rpcData);
            Click(EditButtonBy);
            var form = new EditRiskProfileCheckConfigurationForm(_driver);
            form.Initialize();
            return form;
        }

        public RiskProfileCheckConfigurationPage(IWebDriver driver)
            : base(driver)
        { }


    }
}
