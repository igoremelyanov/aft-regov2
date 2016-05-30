using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class UserPermissionsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private AdminManagerPage _adminManagerPage;
        private RoleManagerPage _roleManagerPage;
        public string DefaultLicensee = "Flycow";
        public string DefaultBrand = "138";
        public string DefaultCurrency = "CAD";
        private SecurityTestHelper _securityTestHelper;
        private Admin _adminData;
        private Guid _defaultLicenseeId;
        private Brand _brand;
        
        public override void BeforeAll()
        {
            base.BeforeAll();
            var brandQueries = _container.Resolve<BrandQueries>();
            _defaultLicenseeId = brandQueries.GetLicensees().First(x => x.Name == DefaultLicensee).Id;
            _brand = brandQueries.GetBrands().First(x => x.Name == DefaultBrand);
            var currencies = brandQueries.GetCurrenciesByBrand(_brand.Id).Select(c => c.Code);
            
            // create a user for default licensee and brand
            _securityTestHelper = _container.Resolve<SecurityTestHelper>();
            _adminData = _securityTestHelper.CreateAdmin(_defaultLicenseeId, new[] { _brand }, currencies, "123456");
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

        [Test]
        [Ignore("Failing unstable on RC-1.0 - Igor, 25-Aiprl-2016")]
        public void Copy_permissions_from_role_option_works()
        {
            // create a source role to copy permissions from
            var sourceRoleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            _roleManagerPage = _dashboardPage.Menu.ClickRoleManagerMenuItem();
            var sourceRoleForm = _roleManagerPage.OpenNewRoleForm();
            sourceRoleForm.SelectPermissions(new [] 
            {
                NewRoleForm.RoleManagerView, NewRoleForm.RoleManagerCreate
            });
            var submittedSorceRole = sourceRoleForm.FillInRequiredFieldsAndSubmit(sourceRoleData);

            // create another role to use the copied permissions
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            submittedSorceRole.CloseTab("View Role");
            var newRoleForm = _roleManagerPage.OpenNewRoleForm();
            var submittedRoleForm = newRoleForm.FillInRequiredFields(roleData);
            newRoleForm.CopyPermissionsFromAnotherRole(sourceRoleData.RoleName);
            newRoleForm.Submit();
            
            // create a user based on the role
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, status:"Active", licensee:roleData.Licensee, brand:DefaultBrand, currency: "CAD");
            
            var adminManagerPage = submittedRoleForm.Menu.ClickAdminManagerMenuItem();
            var newUserForm = adminManagerPage.OpenNewUserForm();
            var submittedUserForm = newUserForm.Submit(userData);

            Assert.AreEqual("User has been successfully created", submittedUserForm.ConfirmationMessage);
            Assert.AreEqual(roleData.RoleName, submittedUserForm.UserRole);

            // login as the user and check that the user can open Role Manager page
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            _roleManagerPage = _dashboardPage.Menu.ClickRoleManagerMenuItem();
            var openedForm = _roleManagerPage.OpenNewRoleForm();

            Assert.AreEqual("New Role", openedForm.TabName);
        }

        [Test]
        [Ignore("Failing unstable on RC-1.0 - Igor, 25-Aiprl-2016")]
        public void Can_change_role_of_user()
        {
            _adminManagerPage = _dashboardPage.Menu.ClickAdminManagerMenuItem();
            var viewUserForm = _adminManagerPage.OpenViewUserForm(_adminData.Username);
            Assert.AreEqual(viewUserForm.UserRole, _adminData.Role.Name);

            viewUserForm.CloseTab("View User");
            var editUserForm = _adminManagerPage.OpenEditUserForm(_adminData.Username);
            var uplatedUserForm = editUserForm.ChangeRole("Licensee");
            _adminData = _container.Resolve<IAdminQueries>().GetAdminByName(uplatedUserForm.Username);
           
            Assert.AreEqual("Licensee", uplatedUserForm.UserRole);
        }
    }
}
