using System;
using System.Globalization;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class LicenseeManagerTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private LicenseeManagerPage _licenseeManagerPage;
        private readonly string _contractHistoryContractStartDate = DateTime.UtcNow.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture);
        private readonly string _contractHistoryContractEndDate = DateTime.UtcNow.AddMonths(5).ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture);
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();            
            _licenseeManagerPage = _dashboardPage.Menu.ClickLicenseeManagerItem();
        }

        [Test]
        [Ignore("Licensee and Products seeding and taking from UGS for now - 16-March-2016,Igor.")]
        public void Can_create_licensee()
        {
            var licenseeName = "Licensee-" + TestDataGenerator.GetRandomString(5);
            var companyName = "Company-" + TestDataGenerator.GetRandomString(5);
            var contractStartDate = DateTime.UtcNow.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture);
            var contractEndDate = DateTime.UtcNow.AddMonths(5).ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture);
            var email = TestDataGenerator.GetRandomEmail();
            var numberOfAllowedBrands = TestDataGenerator.GetRandomNumber(10).ToString(CultureInfo.InvariantCulture);
            var numberOfAllowedWebsites = numberOfAllowedBrands;
            var products = new string[] {"Mock Casino"};
            var currencies = new string[] {"CAD"};
            var countries = new string[] {"Canada"};
            var languages = new string[] {"en-GB"};
            
            var newLicenseeForm = _licenseeManagerPage.OpenNewLicenseeForm();
            var submittedLicenseeForm = newLicenseeForm.Submit(licenseeName, companyName, contractStartDate, contractEndDate, numberOfAllowedBrands, numberOfAllowedWebsites, email, products, currencies,countries,languages);
            
            Assert.AreEqual(licenseeName, submittedLicenseeForm.Licensee);
            Assert.AreEqual(companyName, submittedLicenseeForm.CompanyName);
            Assert.AreEqual(_contractHistoryContractStartDate, submittedLicenseeForm.ContractStartDate);
            Assert.AreEqual(_contractHistoryContractEndDate, submittedLicenseeForm.ContractEndDate);
            Assert.AreEqual(numberOfAllowedBrands, submittedLicenseeForm.NumberOfAllowedBrands);
            Assert.AreEqual(numberOfAllowedWebsites, submittedLicenseeForm.NumberOfAllowedWebsites);
            Assert.AreEqual(products[0], submittedLicenseeForm.AssignedProduct);
            Assert.AreEqual(currencies[0], submittedLicenseeForm.AssignedCurrencies);
            Assert.AreEqual(countries[0], submittedLicenseeForm.AssignedCountries);
            Assert.AreEqual(languages[0], submittedLicenseeForm.AssignedLanguages);
        }

        [Test]
        [Ignore("Licensee and Products seeding and taking from UGS for now - 16-March-2016,Igor.")]
        public void Can_not_duplicate_licensee()
        {
            var licenseeName = "Licensee-" + TestDataGenerator.GetRandomString(5);
            var companyName = "Company-" + TestDataGenerator.GetRandomString(5);
            var contractStartDate = DateTime.UtcNow.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture);
            var contractEndDate = DateTime.UtcNow.AddMonths(5).ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture);
            var email = TestDataGenerator.GetRandomEmail();
            var numberOfAllowedBrands = TestDataGenerator.GetRandomNumber(10).ToString(CultureInfo.InvariantCulture);
            var numberOfAllowedWebsites = numberOfAllowedBrands;
            var products = new string[] { "Mock Casino" };
            var currencies = new string[] { "CAD" };
            var countries = new string[] { "Canada" };
            var languages = new string[] { "en-GB" };

            var newLicenseeForm = _licenseeManagerPage.OpenNewLicenseeForm();
            var submittedLicenseeForm = newLicenseeForm.Submit(licenseeName, companyName, contractStartDate, contractEndDate, numberOfAllowedBrands, numberOfAllowedWebsites, email, products, currencies, countries, languages);
            Assert.AreEqual("The licensee has been successfully created", submittedLicenseeForm.ConfirmationMessage);
            submittedLicenseeForm.CloseTab("View Licensee");

            _dashboardPage.BrandFilter.SelectAll();
            var viewLicenseeForm = _licenseeManagerPage.OpenViewLicenseeForm(licenseeName);
            Assert.AreEqual(licenseeName, viewLicenseeForm.Licensee);
            Assert.AreEqual(companyName, viewLicenseeForm.CompanyName);
            Assert.AreEqual(_contractHistoryContractStartDate, viewLicenseeForm.ContractStartDate);
            Assert.AreEqual(_contractHistoryContractEndDate, viewLicenseeForm.ContractEndDate);
            Assert.AreEqual(numberOfAllowedBrands, viewLicenseeForm.NumberOfAllowedBrands);
            Assert.AreEqual(numberOfAllowedWebsites, viewLicenseeForm.NumberOfAllowedWebsites);
            Assert.AreEqual(products[0], viewLicenseeForm.AssignedProduct);
            Assert.AreEqual(currencies[0], viewLicenseeForm.AssignedCurrencies);
            Assert.AreEqual(countries[0], viewLicenseeForm.AssignedCountries);
            Assert.AreEqual(languages[0], viewLicenseeForm.AssignedLanguages);
            viewLicenseeForm.CloseTabWithTitleContains("View Licensee");

            _licenseeManagerPage.OpenNewLicenseeForm();
            newLicenseeForm.Submit(licenseeName, companyName, contractStartDate, contractEndDate, numberOfAllowedBrands, numberOfAllowedWebsites, email, products, currencies, countries, languages);
            Assert.AreEqual("This name has been used.", newLicenseeForm.NameValidationMessage);
        }

        [Test]
        [Ignore("Failing unstable on RC-1.0 - 26-Aiprl-2016, Igor")]
        public void Can_view_licensee_contract_history()
        {
            var viewLicenseeForm = _licenseeManagerPage.OpenViewLicenseeForm("Flycow");
            _driver.Manage().Window.Maximize();
            
            Assert.AreEqual(_contractHistoryContractStartDate, viewLicenseeForm.ContractHistoryContractStartDate);
            Assert.AreEqual("Active", viewLicenseeForm.Status);
        }
         
        [Test]
        [Ignore("Licensee and Products seeding and taking from UGS for now - 16-March-2016,Igor.")]
        public void Can_edit_licensee_details()
        {
            var licenseeName = "Licensee-" + TestDataGenerator.GetRandomString(5);
            var companyName = "Company-" + TestDataGenerator.GetRandomString(5);
            var contractStartDate = DateTime.UtcNow.ToString("yyyy'/'MM'/'dd");
            var contractEndDate = DateTime.UtcNow.AddMonths(5).ToString("yyyy'/'MM'/'dd");
            var email = TestDataGenerator.GetRandomEmail();
            var numberOfAllowedBrands = TestDataGenerator.GetRandomNumber(10).ToString(CultureInfo.InvariantCulture);
            var numberOfAllowedWebsites = numberOfAllowedBrands;

            //create a licensee
            var newLicenseeForm = _licenseeManagerPage.OpenNewLicenseeForm();
            var submittedLicenseeForm = newLicenseeForm.Submit(licenseeName, companyName, contractStartDate, contractEndDate, numberOfAllowedBrands, numberOfAllowedWebsites,
                email, new string[]{"Mock Casino"}, new string[]{"CAD", "USD"}, new string[]{"Canada", "China"}, new string[]{"en-GB"});
            submittedLicenseeForm.CloseTab("View Licensee");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _licenseeManagerPage = _dashboardPage.Menu.ClickLicenseeManagerItem();            

            // edit licensee details
            var licenseeNameEdited = licenseeName + "edited";
            var companyNameEdited = companyName + "edited";
            var contractStartDateEdited = DateTime.UtcNow.AddMonths(1).ToString("yyyy'/'MM'/'dd");
            var contractEndDateEdited = DateTime.UtcNow.AddMonths(5).ToString("yyyy'/'MM'/'dd");
            var emailEdited = string.Format("{0}" + email, "edited");

            var editLicenseeForm = _licenseeManagerPage.OpenEditLicenseeForm(licenseeName);
            editLicenseeForm.ClearFieldsOnForm();
            var licenseeData = TestDataGenerator.EditLicenseeData(
                licenseeNameEdited, companyNameEdited, contractStartDateEdited, emailEdited, contractEndDateEdited,
                new string[] { "Mock Sport Bets" }, new string[] { "ALL", "BDT" }, new string[] { "Great Britain" }, new string[] { "zh-TW" });
            
            var editedForm = editLicenseeForm.Submit(licenseeData);
            
            Assert.AreEqual("The licensee has been successfully updated", editedForm.ConfirmationMessage);
            Assert.AreEqual(licenseeNameEdited, editedForm.Licensee);
        }

        [Test]
        [Ignore("Licensee and Products seeding and taking from UGS for now - 16-March-2016,Igor.")]
        public void Can_renew_expired_licensee_contract()
        {
            var licenseeName = "Licensee-" + TestDataGenerator.GetRandomString(5);
            var companyName = "Company-" + TestDataGenerator.GetRandomString(5);
            var contractStartDate = DateTime.UtcNow.AddMonths(-2).ToString("yyyy'/'MM'/'dd");
            var contractEndDate = DateTime.UtcNow.AddDays(2).ToString("yyyy'/'MM'/'dd");
            var email = TestDataGenerator.GetRandomEmail();
            var numberOfAllowedBrands = TestDataGenerator.GetRandomNumber(10).ToString(CultureInfo.InvariantCulture);
            var numberOfAllowedWebsites = numberOfAllowedBrands;

            // create a licensee
            var newLicenseeForm = _licenseeManagerPage.OpenNewLicenseeForm();
            var submittedLicenseeForm = newLicenseeForm.Submit(licenseeName, companyName, contractStartDate, contractEndDate, numberOfAllowedBrands, numberOfAllowedWebsites, email);
            submittedLicenseeForm.CloseTab("View Licensee");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _licenseeManagerPage = _dashboardPage.Menu.ClickLicenseeManagerItem();            

            //activate
            var activateDialog = _licenseeManagerPage.OpenActivateLicenseeDialog(licenseeName);
            var submittedActivateDialog = activateDialog.Activate("test");
            _licenseeManagerPage = submittedActivateDialog.Close();

            //edit end date
            var editLicenseePage = _licenseeManagerPage.OpenEditLicenseeForm(licenseeName);
            var viewLicenseePage = editLicenseePage.Submit(new LicenseeData
            {
                ContractEnd = DateTime.UtcNow.AddDays(-3).ToString("yyyy'/'MM'/'dd"),
                Remarks = "test"
            });
            viewLicenseePage.CloseTab("View Licensee");

            //renew
            var newContractDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy'/'MM'/'dd");
            var newContractEnd = DateTime.UtcNow.AddMonths(11).ToString("yyyy'/'MM'/'dd");
            var renewContractForm = _licenseeManagerPage.OpenRenewContractForm(licenseeName);

            var submittedForm = renewContractForm.Submit(newContractDate, newContractEnd);
            var contractStatus = submittedForm.GetStatus(newContractDate);

            Assert.AreEqual("The licensee contract has been renewed successfully", submittedForm.ConfirmationMessage);
            Assert.AreEqual("Active", contractStatus);
        }
    }    
}
