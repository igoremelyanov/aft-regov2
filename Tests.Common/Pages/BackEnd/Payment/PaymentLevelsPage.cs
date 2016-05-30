using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentLevelsPage : BackendPageBase
    {
        public PaymentLevelsPage(IWebDriver driver) : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "payment-levels-name-search", "search-button");
            }
        }

        public string Title
        {
            get { return _driver.FindElementWait(By.XPath("//h5[text()='Payment Level Manager']")).Text; } 
        }

        public NewPaymentLevelForm OpenNewPaymentLevelForm()
        {
            var newButton = _driver.FindElementWait(By.XPath("//div[@id='payment-levels-list']//button[contains(@data-bind, 'click: openAddTab')]"));
            newButton.Click();
            Thread.Sleep(5000); //wait for New Payment level form
            var form = new NewPaymentLevelForm(_driver);
            return form;
        }

        public EditPaymentLevelForm OpenEditForm(string paymentLevelName)
        {
            Grid.SelectRecord(paymentLevelName);
            var editButton = _driver.FindElementWait(By.XPath("//div[@id='payment-levels-list']//button[contains(@data-bind, 'click: openEditTab')]"));
            editButton.Click();
            var editForm = new EditPaymentLevelForm(_driver);
            editForm.Initialize();
            return editForm;
        }
    }
}