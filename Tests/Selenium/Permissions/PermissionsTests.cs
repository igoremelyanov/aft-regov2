using System.Threading;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class PermissionsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

        [Test]
        public void Can_create_user_with_permission()
        {
            // create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, status:"Active", licensee:DefaultLicensee, brand:DefaultBrand, currency:"ALL");

            _driver.CreateUser(roleData, userData, new[]
            {
                NewRoleForm.AdminManagerView, NewRoleForm.AdminManagerCreate
            });
            Thread.Sleep(5000); //wait for new User event proceeds.

            //log in as the user
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var adminManagerPage = _dashboardPage.Menu.ClickAdminManagerMenuItem();
            var newForm = adminManagerPage.OpenNewUserForm();

            Assert.AreEqual("New User", newForm.TabName);
        }
        
        [Test]
        public void Cannot_view_language_without_permission()
        {
            //create a user
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                role: "FraudOfficer", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "ALL");
            _driver.CreateUserBasedOnPredefinedRole(userData);
            Thread.Sleep(5000); //wait for new User event proceeds.

            //log in as the user
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var languageManagerMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.LanguageManager);

            Assert.IsFalse(languageManagerMenuItemVisible);
        }

        [Test]
        [Ignore("Until VladS fixes - AFREGO-4285, 22-Feb-2016, Igor")]
        public void Cannot_manage_language_without_permission()
        {
            //create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, status:"Active", licensee:roleData.Licensee, brand:DefaultBrand, currency: "ALL");
            
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.LanguageManagerView });
            Thread.Sleep(5000); //wait for new User event proceeds.

            //log in as the user
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var languageManagerPage = _dashboardPage.Menu.ClickLanguageManagerMenuItem();
            foreach (var btnName in new[] {"new", "edit", "activate", "deactivate"})
            {
                var button = languageManagerPage.GetButton(btnName);
                Assert.That(!button.Displayed);
            }
        }
    }
}
