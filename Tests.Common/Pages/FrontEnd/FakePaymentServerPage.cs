using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class FakePaymentServerPage : FrontendPageBase
    {
        #region Locators

        internal static readonly By InputAmountBy = By.XPath("//input[contains(@name,'Amount')]");
        internal static readonly By NotifyAndRedirectButtonBy = By.XPath("//input[contains(@value,'Redirect')]");

        //close button from the next fake page - there is no sense to create the separate page
        internal static readonly By CloseButtonBy = By.XPath("//button[contains(@onclick,'window.close()')]");

        #endregion

        #region Elements

        public IWebElement InputAmount
        {
            get { return _driver.FindElementWait(InputAmountBy); }
        }

        public IWebElement NotifyAndRedirectButton
        {
            get { return _driver.FindElementWait(NotifyAndRedirectButtonBy); }
        }

        public IWebElement CloseButton
        {
            get { return _driver.FindElementWait(CloseButtonBy); }
        }

        #endregion

        private string _winHandleBefore;

        public FakePaymentServerPage(IWebDriver driver) : base(driver)
        {
            _winHandleBefore = _driver.CurrentWindowHandle;
            _driver.SwitchTo().Window(_driver.WindowHandles.Last());
            _driver.WaitForJavaScript();
        }

        public string DepositWindowUrl
        {
            get
            {
                var url = _driver.Url;
                return url;
            }
        }

        public string AlertMessage
        {
            get
            {
                var alert = _driver.SwitchTo().Alert();
                var msg = alert.Text;
                alert.Accept();
                return msg;
            }
        }

        
       public void SubmitReturn()
        {
            var submitButton = _driver.FindElementWait(By.XPath("//input[@value='Notify and Redirect']"));
            submitButton.Click();
        }

        public void SubmitNotify()
        {
            var submitButton = _driver.FindElementWait(By.XPath("//input[@value='Notify Only']"));
            submitButton.Click();
        }

        public void Cancel()
        {
            var submitButton = _driver.FindElementWait(By.XPath("//input[@value='Cancel']"));
            submitButton.Click();
        }

        public void BackToMemberSite()
        {
            _driver.SwitchTo().Window(_winHandleBefore);
        }

        public string OrderId
        {
            get
            {
                var element = _driver.FindElementWait(By.XPath("//input[@name='OrderId']"));
                return element.GetAttribute("value");
            }
        }

        public void NotifyAndRedirect()
        {
            _driver.WaitForElementClickable(NotifyAndRedirectButton);
            NotifyAndRedirectButton.Click();
            _driver.WaitAndClickElement(CloseButton);

            BackToMemberSite();
            _driver.WaitForJavaScript();
        }

        public string GetAmountValue()
        {
           return InputAmount.GetAttribute("value");
       }
}
}