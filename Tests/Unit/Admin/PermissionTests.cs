using System;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Admin
{
    class PermissionTests : PermissionsTestsBase
    {
        private IAdminQueries _adminQueries;
        private IAdminCommands _adminCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _adminQueries = Container.Resolve<IAdminQueries>();
            _adminCommands = Container.Resolve<IAdminCommands>();
        }

        [Test]
        public void Cannot_execute_admin_services_without_permissions()
        {
            /* Arrange */
            LogWithNewAdmin(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _adminQueries.GetAdmins());
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.CreateAdmin(new AddAdminData()));
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.UpdateAdmin(new EditAdminData()));
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.ChangePassword(new Guid(), "password"));
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.Activate(new ActivateUserData(new Guid(), string.Empty)));
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.Deactivate(new DeactivateUserData(new Guid(), string.Empty)));
        }

        [Test]
        public void Cannot_create_admin_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true).Id, brandTestHelper.CreateBrand(licensee, isActive: true).Id };
            var currencies = new[] {brandTestHelper.CreateCurrency("CAD", "Canadian Dollar").Code};
            var role = securityTestHelper.CreateRole(new[] {licensee.Id});

            var userData = new AddAdminData
            {
                Username = "User123",
                FirstName = "User",
                LastName = "123",
                Password = "Password123",
                Language = "English",
                IsActive = true,
                AssignedLicensees = new[] {licensee.Id},
                AllowedBrands = brands,
                Currencies = currencies,
                RoleId = role.Id
            };

            LogWithNewAdmin(Modules.AdminManager, Permissions.Create);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.CreateAdmin(userData));
        }

        [Test]
        public void Cannot_update_admin_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true).Id, brandTestHelper.CreateBrand(licensee, isActive: true).Id };
            var currencies = new[] { brandTestHelper.CreateCurrency("CAD", "Canadian Dollar").Code };
            var role = securityTestHelper.CreateRole(new[] { licensee.Id });

            var addUserData = new AddAdminData
            {
                Username = "User123",
                FirstName = "User",
                LastName = "123",
                Password = "Password123",
                Language = "English",
                IsActive = true,
                AssignedLicensees = new[] { licensee.Id },
                AllowedBrands = brands,
                Currencies = currencies,
                RoleId = role.Id
            };

            var user = _adminCommands.CreateAdmin(addUserData);

            var editUserData = Mapper.DynamicMap<EditAdminData>(addUserData);
            editUserData.Id = user.Id;

            LogWithNewAdmin(Modules.AdminManager, Permissions.Update);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.UpdateAdmin(editUserData));
        }

        [Test]
        public void Cannot_change_admin_password_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true), brandTestHelper.CreateBrand(licensee, isActive: true) };
            var user = securityTestHelper.CreateAdmin(licensee.Id, brands);

            LogWithNewAdmin(Modules.AdminManager, Permissions.Update);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.ChangePassword(user.Id, "password"));
        }

        [Test]
        public void Cannot_activate_admin_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true), brandTestHelper.CreateBrand(licensee, isActive: true) };
            var user = securityTestHelper.CreateAdmin(licensee.Id, brands);

            LogWithNewAdmin(Modules.AdminManager, Permissions.Update);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.Activate(new ActivateUserData(user.Id, string.Empty)));
        }

        [Test]
        public void Cannot_deactivate_admin_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true), brandTestHelper.CreateBrand(licensee, isActive: true) };
            var user = securityTestHelper.CreateAdmin(licensee.Id, brands);

            LogWithNewAdmin(Modules.AdminManager, Permissions.Update);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _adminCommands.Deactivate(new DeactivateUserData(user.Id, string.Empty)));
        }
    }
}
