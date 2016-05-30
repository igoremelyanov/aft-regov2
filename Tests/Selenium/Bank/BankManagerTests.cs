using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Payment;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Bank
{
    [Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
    internal class BankManagerTests : SeleniumBaseForAdminWebsite
    {
        private BankManagerPage _banksManagerPage;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
        }

        public override void AfterAll()
        {
            base.AfterAll();
            _driver.Logout();
        }

        [Test]
        public void Can_create_and_view_bank()
        {
            //create bank
            var bankName = "Bank" + TestDataGenerator.GetRandomString(3);
            var bankId = TestDataGenerator.GetRandomString(5);

            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _banksManagerPage = dashboardPage.Menu.ClickBanksItem();

            var newBankForm = _banksManagerPage.OpenNewBankForm();

            var submittedBankForm = newBankForm.SubmitWithLicensee(
                "Flycow",
                "138",
                bankId,
                bankName,
                "China",
                "new bank");

            Assert.AreEqual("The bank has been successfully created", submittedBankForm.ConfirmationMessage);
            submittedBankForm.CloseTab("View");

            //view and check bamk information
            var viewBankForm = _banksManagerPage.OpenViewBankForm(bankName);
            Assert.AreEqual("Flycow", viewBankForm.LicenseeValue);
            Assert.AreEqual("138", viewBankForm.BrandValue);
            Assert.AreEqual(bankId, viewBankForm.BankIdValue);
            Assert.AreEqual(bankName, viewBankForm.BankNameValue);
            Assert.AreEqual("China", viewBankForm.CountryValue);
            Assert.AreEqual("new bank", viewBankForm.RemarksValue);
        }
       
        [Test]
        public void Can_edit_bank()
        {
            var bankName = "Bank" + TestDataGenerator.GetRandomString(3);
            var bankId = TestDataGenerator.GetRandomString(5);
           
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _banksManagerPage = dashboardPage.Menu.ClickBanksItem();
            
            var newBankForm = _banksManagerPage.OpenNewBankForm();
            
            var submittedBankForm = newBankForm.SubmitWithLicensee(
                "Flycow", 
                "138", 
                bankId, 
                bankName, 
                "China",
                "new bank");

            Assert.AreEqual("The bank has been successfully created", submittedBankForm.ConfirmationMessage);
            submittedBankForm.CloseTab("View");
            
            // edit bank details
            var editForm = _banksManagerPage.OpenEditBankForm(bankName);
            var newBankName = bankName + "edited";
            var submittedEditForm = editForm.Submit("Flycow", "138", newBankName);

            Assert.AreEqual("The bank has been successfully updated", submittedEditForm.ConfirmationMessage);
            Assert.AreEqual(newBankName, submittedEditForm.BankNameValue);
        }
    }
}
