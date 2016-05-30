using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class BalanceDetailsPage : FrontendPageBase
    {
        public BalanceDetailsPage(IWebDriver driver) : base(driver) { }

        public string GetBonusBalance(string walletId)
        {
            return
               _driver.FindElement(By.XPath("//span[@id='" + walletId + ".bonusBalance']")).Text;
        }


        public string GetMainBalance(string walletId)
        {
            return
                    _driver.FindElement(By.XPath("//span[@id='" + walletId + ".mainBalance']")).Text;
        }


        public BalanceDetailsPage OpenFundTransferSection()
        {
            _driver.Url += "#fundIn";

            return this;
        }

        
    }
}
