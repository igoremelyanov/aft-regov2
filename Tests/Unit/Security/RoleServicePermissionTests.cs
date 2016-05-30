using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class RoleServicePermissionTests : PermissionsTestsBase
    {
        private RoleService _roleService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _roleService = Container.Resolve<RoleService>();
        }

        [Test]
        public void Cannot_execute_RoleService_without_permissions()
        {
            /* Arrange */
            LogWithNewAdmin(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _roleService.GetRoles());
            Assert.Throws<InsufficientPermissionsException>(() => _roleService.CreateRole(new AddRoleData()));
            Assert.Throws<InsufficientPermissionsException>(() => _roleService.UpdateRole(new EditRoleData()));
        }
    }
}
