using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AutoMapper;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Admin
{
    class AdminCommandsTests : SecurityTestsBase
    {
        [Test]
        public void Can_create_admin()
        {
            var admin = SecurityTestHelper.CreateAdmin();

            Assert.IsNotNull(admin);
            Assert.False(admin.Id == Guid.Empty);
        }

        [Test]
        public void Cannot_create_admin_without_licensees()
        {
            /*** Arrange ***/
            var userData = CreateAddAdminData();

            /*** Act ***/
            Action action = () => AdminCommands.CreateAdmin(userData);

            action.ShouldThrow<RegoException>()
                .Where(x => x.Message.Contains("licenseesRequired"));
        }

        [Test]
        public void Cannot_create_admin_without_brands()
        {
            /*** Arrange ***/
            var licensees = BrandQueries.GetLicensees().Select(l => l.Id);
            var userData = CreateAddAdminData(licensees);

            /*** Act ***/
            Action action = () => AdminCommands.CreateAdmin(userData);

            /*** Assert ***/
            action.ShouldThrow<RegoException>()
                .Where(x => x.Message.Contains("brandsRequired"));
        }

        [Test]
        public void Can_update_admin()
        {
            var admin = SecurityTestHelper.CreateAdmin();

            var firstName = TestDataGenerator.GetRandomString();
            var lastName = TestDataGenerator.GetRandomString();
            var status = true;
            var language = TestDataGenerator.GetRandomString();
            var description = TestDataGenerator.GetRandomString();

            admin.FirstName = firstName;
            admin.LastName = lastName;
            admin.IsActive = status;
            admin.Language = language;
            admin.Description = description;

            var adminData = Mapper.DynamicMap<EditAdminData>(admin);

            adminData.AllowedBrands = admin.AllowedBrands.Select(b => b.Id).ToList();
            adminData.AssignedLicensees = admin.Licensees.Select(l => l.Id).ToList();
            adminData.Password = TestDataGenerator.GetRandomString();

            AdminCommands.UpdateAdmin(adminData);

            admin = AdminQueries.GetAdminById(admin.Id);

            Assert.True(admin.FirstName == firstName);
            Assert.True(admin.LastName == lastName);
            Assert.True(admin.IsActive == status);
            Assert.True(admin.Language == language);
            Assert.True(admin.Description == description);
        }

        [Test]
        public void Cannot_update_admin_without_licensees()
        {
            /*** Arrange ***/
            var admin = SecurityTestHelper.CreateAdmin();

            var adminData = Mapper.DynamicMap<EditAdminData>(admin);

            adminData.AllowedBrands = new[] {Brand.Id};
            adminData.AssignedLicensees = null;

            /*** Act ***/
            Action action = () => AdminCommands.UpdateAdmin(adminData);

            /*** Assert ***/
            action.ShouldThrow<RegoException>()
                .Where(x => x.Message.Contains("licenseesRequired"));
        }

        [Test]
        public void Cannot_update_admin_without_brands()
        {
            /*** Arrange ***/
            var admin = SecurityTestHelper.CreateAdmin();

            var adminData = Mapper.DynamicMap<EditAdminData>(admin);

            adminData.AssignedLicensees = admin.Licensees.Select(l => l.Id).ToList();
            adminData.AllowedBrands = null;

            /*** Act ***/
            Action action = () => AdminCommands.UpdateAdmin(adminData);

            /*** Assert ***/
            action.ShouldThrow<RegoException>()
                .Where(x => x.Message.Contains("brandsRequired"));
        }

        [Test]
        public void Can_change_password()
        {
            var admin = SecurityTestHelper.CreateAdmin();

            var authRepository = Container.Resolve<IAuthRepository>();
            var initialPasswordValue = authRepository.Actors.Single(a => a.Id == admin.Id).EncryptedPassword;

            AdminCommands.ChangePassword(admin.Id, TestDataGenerator.GetRandomString());

            var updatedPasswordValue = authRepository.Actors.Single(a => a.Id == admin.Id).EncryptedPassword;

            Assert.AreNotEqual(initialPasswordValue, updatedPasswordValue);
        }

        [Test]
        public void Can_activate()
        {
            // *** Arrange ***
            var admin = SecurityTestHelper.CreateAdmin();

            // *** Act ***
            AdminCommands.Activate(new ActivateUserData(admin.Id, string.Empty));

            // *** Assert ***
            var activatedAdmin = AdminQueries.GetAdminById(admin.Id);

            Assert.IsNotNull(activatedAdmin);
            Assert.True(activatedAdmin.IsActive == true);
        }

        [Test]
        public void Can_deactivate()
        {
            // *** Arrange ***
            var admin = SecurityTestHelper.CreateAdmin();

            AdminCommands.Activate(new ActivateUserData(admin.Id, string.Empty));

            // *** Act ***
            AdminCommands.Deactivate(new DeactivateUserData(admin.Id, string.Empty));

            // *** Assert ***
            var deactivatedAdmin = AdminQueries.GetAdminById(admin.Id);

            Assert.IsNotNull(deactivatedAdmin);
            Assert.True(deactivatedAdmin.IsActive == false);
        }

        [Test]
        public void Can_add_allowed_brand_to_admin()
        {
            // *** Arrange ***
            var admin = SecurityTestHelper.CreateAdmin();

            var brand = BrandHelper.CreateBrand();

            // *** Act ***
            AdminCommands.AddBrandToAdmin(admin.Id, brand.Id);

            // *** Assert ***
            var createdAdmin = SecurityRepository.Admins.Include(u => u.AllowedBrands).FirstOrDefault(u => u.Id == admin.Id);

            Assert.IsNotNull(createdAdmin);
            Assert.IsNotNull(createdAdmin.AllowedBrands);
            Assert.True(createdAdmin.AllowedBrands.Any(b => b.Id == brand.Id));
        }

        private AddAdminData CreateAddAdminData(IEnumerable<Guid> licensees = null)
        {
            var role = SecurityTestHelper.CreateRole();
            var userName = "User-" + TestDataGenerator.GetRandomString(5);

            var addAdminData = new AddAdminData
            {
                Username = userName,
                FirstName = userName,
                LastName = userName,
                Password = TestDataGenerator.GetRandomString(),
                Language = "English",
                IsActive = true,
                RoleId = role.Id,
                AssignedLicensees = licensees != null ? licensees.ToList() : null
            };

            return addAdminData;
        }
    }
}