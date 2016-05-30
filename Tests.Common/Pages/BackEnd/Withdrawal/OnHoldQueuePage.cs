using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Withdrawal
{
    //represents withdrawal On Hold Queue
    public class OnHoldQueuePage : WithdrawalParentQueue
    {
        internal static readonly By ButtonUnverifyBy = By.XPath("//button[contains(@name,'unverify')]");


        public SubmitWithdrawalProcessingForm OpenUnverifyForm(IWebElement record)
        {
            record.Click();
            Click(ButtonUnverifyBy);
            return new SubmitWithdrawalProcessingForm(_driver);
        }
        public OnHoldQueuePage(IWebDriver driver)
            : base(driver)
        {
        }

 }
}
