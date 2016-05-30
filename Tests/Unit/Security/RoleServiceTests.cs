using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AutoMapper;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class RoleServiceTests : SecurityTestsBase
    {
        [Test]
        public void Can_create_role()
        {
            var role = SecurityTestHelper.CreateRole();

            Assert.IsNotNull(role);
            Assert.False(role.Id == Guid.Empty);
        }

        [Test]
        public void Can_update_role()
        {
            // *** Arrange ***
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(roleId: role.Id);

            role.Code = TestDataGenerator.GetRandomString();
            role.Name = TestDataGenerator.GetRandomString();
            role.Description = TestDataGenerator.GetRandomString();

            var roleData = Mapper.DynamicMap<EditRoleData>(role);
            roleData.CheckedPermissions = new List<Guid>();

            SecurityTestHelper.SignInAdmin(admin);

            // *** Act ***
            var roleService = Container.Resolve<RoleService>();
            roleService.UpdateRole(roleData);

            // *** Assert ***
            var updatedRole = roleService.GetRoleById(role.Id);

            Assert.True(updatedRole.Code == role.Code);
            Assert.True(updatedRole.Name == role.Name);
            Assert.True(updatedRole.Description == role.Description);
            Assert.True(updatedRole.UpdatedBy.Id == admin.Id);
            Assert.True(updatedRole.UpdatedDate.HasValue);
        }
    }
}
