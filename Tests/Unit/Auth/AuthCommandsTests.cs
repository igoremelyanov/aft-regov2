using System;
using System.Linq;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Auth
{
    public class AuthCommandsTests : AdminWebsiteUnitTestsBase
    {
        private IAuthQueries _authQueries;
        private IAuthCommands _authCommands;
        private IAuthRepository _authRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _authQueries = Container.Resolve<IAuthQueries>();
            _authCommands = Container.Resolve<IAuthCommands>();
            _authRepository = Container.Resolve<IAuthRepository>();
        }

        [Test]
        public void Can_create_actor()
        {
            var model = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Username = TestDataGenerator.GetRandomString(),
                Password = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreateActor(model);

            var actor = _authRepository.Actors.SingleOrDefault(a => a.Id == model.ActorId);

            actor.Should().NotBeNull();
            actor.Username.Should().Be(model.Username);
            actor.EncryptedPassword.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void Can_change_actors_password()
        {
            var createActor = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = TestDataGenerator.GetRandomString(),
                Username = TestDataGenerator.GetRandomString()
            };
            _authCommands.CreateActor(createActor);

            var actor = _authRepository.Actors.Single(a => a.Id == createActor.ActorId);
            var encryptedPassword = actor.EncryptedPassword;

            var changePassword = new ChangePassword
            {
                ActorId = createActor.ActorId,
                NewPassword = TestDataGenerator.GetRandomString()
            };
            _authCommands.ChangePassword(changePassword);

            actor = _authRepository.Actors.Single(a => a.Id == createActor.ActorId);

            actor.EncryptedPassword.Should().NotBe(encryptedPassword);
        }

        [Test]
        public void Can_create_role()
        {
            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });

            var createRole = new CreateRole
            {
                RoleId = Guid.NewGuid(),
                Permissions = _authQueries.GetPermissions().Select(p => p.Id).ToList()
            };
            _authCommands.CreateRole(createRole);

            var role = _authRepository.Roles.SingleOrDefault(r => r.Id == createRole.RoleId);

            role.Should().NotBeNull();
            role.Permissions.Count.Should().Be(createRole.Permissions.Count);
        }

        [Test]
        public void Can_update_role()
        {
            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });

            var createRole = new CreateRole
            {
                RoleId = Guid.NewGuid(),
                Permissions = _authQueries.GetPermissions().Select(p => p.Id).ToList()
            };
            _authCommands.CreateRole(createRole);

            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test1",
                Module = "Test1"
            });
            var updateRole = new UpdateRole
            {
                RoleId = createRole.RoleId,
                Permissions = _authQueries.GetPermissions().Select(p => p.Id).ToList()
            };
            _authCommands.UpdateRole(updateRole);

            var role = _authRepository.Roles.Single(r => r.Id == createRole.RoleId);

            role.Permissions.Count.Should().Be(updateRole.Permissions.Count);
        }

        [Test]
        public void Can_create_permission()
        {
            var model = new CreatePermission
            {
                Module = TestDataGenerator.GetRandomString(),
                Name = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreatePermission(model);

            var permission = _authRepository.Permissions.SingleOrDefault(p => p.Name == model.Name && p.Module == model.Module);

            permission.Should().NotBeNull();
        }

        [Test]
        public void Can_assign_role_to_actor()
        {
            var createActor = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Username = TestDataGenerator.GetRandomString(),
                Password = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreateActor(createActor);

            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });

            var createRole = new CreateRole
            {
                RoleId = Guid.NewGuid(),
                Permissions = _authQueries.GetPermissions().Select(p => p.Id).ToList()
            };
            _authCommands.CreateRole(createRole);

            _authCommands.AssignRoleToActor(new AssignRole
            {
                ActorId = createActor.ActorId,
                RoleId = createRole.RoleId
            });

            var actor = _authRepository.Actors.Single(a => a.Id == createActor.ActorId);

            actor.Role.Should().NotBeNull();
            actor.Role.Id.Should().Be(createRole.RoleId);
        }
    }
}
