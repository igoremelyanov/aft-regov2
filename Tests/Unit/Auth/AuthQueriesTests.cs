using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Auth
{
    public class AuthQueriesTests : AdminWebsiteUnitTestsBase
    {
        private IAuthQueries _authQueries;
        private IAuthCommands _authCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _authQueries = Container.Resolve<IAuthQueries>();
            _authCommands = Container.Resolve<IAuthCommands>();
        }

        [Test]
        public void Can_verify_permission_for_admin()
        {
            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });
            var permissions = _authQueries.GetPermissions().Select(p => p.Id);
            var roleId = Guid.NewGuid();
            _authCommands.CreateRole(new CreateRole
            {
                RoleId = roleId,
                Permissions = permissions.ToList()
            });
            var actorId = Guid.NewGuid();
            var createActorModel = new CreateActor
            {
                ActorId = actorId,
                Password = TestDataGenerator.GetRandomString(),
                Username = TestDataGenerator.GetRandomString()
            };
            _authCommands.CreateActor(createActorModel);
            _authCommands.AssignRoleToActor(new AssignRole
            {
                ActorId = createActorModel.ActorId,
                RoleId = roleId
            });

            var actorHasPermission = _authQueries.VerifyPermission(actorId, "Test", "Test");
            var actorHasNoPermission = _authQueries.VerifyPermission(actorId, "Invalid", "Invalid");

            actorHasPermission.Should().BeTrue();
            actorHasNoPermission.Should().BeFalse();
        }

        [Test]
        public void Invalid_actorId_is_not_valid_for_login()
        {
            var model = new LoginActor();

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ActorDoesNotExist.ToString());
        }

        [Test]
        public void Invalid_password_is_not_valid_for_login()
        {
            var createActor = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = TestDataGenerator.GetRandomString(),
                Username = TestDataGenerator.GetRandomString()
            };
            _authCommands.CreateActor(createActor);

            var loginActor = new LoginActor
            {
                ActorId = createActor.ActorId,
                Password = TestDataGenerator.GetRandomString()
            };

            var result = _authQueries.GetValidationResult(loginActor);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ActorPasswordIsNotValid.ToString());
        }

        [Test]
        public void Duplicate_actorId_is_not_valid_for_actor_creation()
        {
            var model = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = TestDataGenerator.GetRandomString(),
                Username = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreateActor(model);

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ActorAlreadyCreated.ToString());
        }

        [Test]
        public void Empty_username_is_not_valid_for_actor_creation()
        {
            var model = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = TestDataGenerator.GetRandomString(),
                Username = null
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.UsernameIsEmpty.ToString());
        }

        [Test]
        public void Empty_password_is_not_valid_for_actor_creation()
        {
            var model = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = string.Empty,
                Username = TestDataGenerator.GetRandomString()
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.PasswordIsEmpty.ToString());
        }

        [Test]
        public void Empty_module_name_is_not_valid_for_permission_creation()
        {
            var model = new CreatePermission
            {
                Module = string.Empty,
                Name = TestDataGenerator.GetRandomString()
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ModuleOrPermissionNameIsEmpty.ToString());
        }

        [Test]
        public void Empty_permission_name_is_not_valid_for_permission_creation()
        {
            var model = new CreatePermission
            {
                Module = TestDataGenerator.GetRandomString(),
                Name = null
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ModuleOrPermissionNameIsEmpty.ToString());
        }

        [Test]
        public void Duplicate_permission_and_module_name_combination_is_not_valid_for_permission_creation()
        {
            var model = new CreatePermission
            {
                Module = "Test",
                Name = "Test"
            };

            _authCommands.CreatePermission(model);

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.DuplicatePermission.ToString());
        }

        [Test]
        public void Invalid_actorId_is_not_valid_for_password_change()
        {
            var changePassword = new ChangePassword
            {
                ActorId = Guid.NewGuid(),
                NewPassword = TestDataGenerator.GetRandomString()
            };

            var result = _authQueries.GetValidationResult(changePassword);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ActorDoesNotExist.ToString());
        }

        [Test]
        public void Empty_password_is_not_valid_for_password_change()
        {
            var createActor = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = TestDataGenerator.GetRandomString(),
                Username = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreateActor(createActor);

            var changePassword = new ChangePassword
            {
                ActorId = createActor.ActorId,
                NewPassword = string.Empty
            };

            var result = _authQueries.GetValidationResult(changePassword);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.PasswordIsEmpty.ToString());
        }

        [Test]
        public void Same_passwords_are_not_valid_for_password_change()
        {
            var createActor = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Password = TestDataGenerator.GetRandomString(),
                Username = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreateActor(createActor);

            var changePassword = new ChangePassword
            {
                ActorId = createActor.ActorId,
                NewPassword = createActor.Password
            };

            var result = _authQueries.GetValidationResult(changePassword);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.PasswordsMatch.ToString());
        }

        [Test]
        public void Duplicate_roleId_is_not_valid_for_role_creation()
        {
            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });

            var model = new CreateRole
            {
                RoleId = Guid.NewGuid(),
                Permissions = _authQueries.GetPermissions().Select(p => p.Id).ToList()
            };
            _authCommands.CreateRole(model);

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.RoleAlreadyCreated.ToString());
        }

        [Test]
        public void Not_registered_permission_is_not_valid_for_role_creation()
        {
            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });

            var model = new CreateRole
            {
                RoleId = Guid.NewGuid(),
                Permissions = new List<Guid>
                {
                    Guid.NewGuid(),
                    _authQueries.GetPermissions().Single().Id
                }
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.PermissionIsNotRegistered.ToString());
        }

        [Test]
        public void Invalid_roleId_is_not_valid_for_role_editing()
        {
            _authCommands.CreatePermission(new CreatePermission
            {
                Name = "Test",
                Module = "Test"
            });

            var model = new UpdateRole
            {
                RoleId = Guid.NewGuid(),
                Permissions = _authQueries.GetPermissions().Select(p => p.Id).ToList()
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.RoleNotFound.ToString());
        }

        [Test]
        public void Not_registered_permission_is_not_valid_for_role_editing()
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

            var model = new UpdateRole
            {
                RoleId = createRole.RoleId,
                Permissions = new List<Guid>
                {
                    Guid.NewGuid(),
                    _authQueries.GetPermissions().Single().Id
                }
            };

            var result = _authQueries.GetValidationResult(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.PermissionIsNotRegistered.ToString());
        }

        [Test]
        public void Invalid_actorId_is_not_valid_for_role_assignment()
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

            var assignRole = new AssignRole
            {
                ActorId = Guid.NewGuid(),
                RoleId = createRole.RoleId
            };

            var result = _authQueries.GetValidationResult(assignRole);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.ActorDoesNotExist.ToString());

        }

        [Test]
        public void Invalid_roleId_is_not_valid_for_role_assignment()
        {
            var createActor = new CreateActor
            {
                ActorId = Guid.NewGuid(),
                Username = TestDataGenerator.GetRandomString(),
                Password = TestDataGenerator.GetRandomString()
            };

            _authCommands.CreateActor(createActor);

            var assignRole = new AssignRole
            {
                ActorId = createActor.ActorId,
                RoleId = Guid.NewGuid()
            };

            var result = _authQueries.GetValidationResult(assignRole);

            result.IsValid.Should().BeFalse();
            result.Errors.Single().ErrorMessage.Should().Be(ErrorsCodes.RoleNotFound.ToString());
        }
    }
}
