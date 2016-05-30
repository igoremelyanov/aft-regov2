using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Auth.Validators;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Auth.ApplicationServices
{
    public class AuthQueries : IAuthQueries
    {
        private readonly IAuthRepository _repository;

        public AuthQueries(IAuthRepository repository)
        {
            _repository = repository;
        }

        public ValidationResult GetValidationResult(LoginActor model)
        {
            var validator = new LoginValidator(_repository);

            return validator.Validate(model);
        }

        public ValidationResult GetValidationResult(CreateActor model)
        {
            var validator = new CreateActorValidator(_repository);

            return validator.Validate(model);
        }

        public ValidationResult GetValidationResult(CreatePermission model)
        {
            var validator = new CreatePermissionValidator(_repository);

            return validator.Validate(model);
        }

        public ValidationResult GetValidationResult(ChangePassword model)
        {
            var validator = new ChangePasswordValidator(_repository);

            return validator.Validate(model);
        }

        public ValidationResult GetValidationResult(CreateRole model)
        {
            var validator = new CreateRoleValidator(_repository);

            return validator.Validate(model);
        }

        public ValidationResult GetValidationResult(UpdateRole model)
        {
            var validator = new UpdateRoleValidator(_repository);

            return validator.Validate(model);
        }

        public ValidationResult GetValidationResult(AssignRole model)
        {
            var validator = new AssignRoleValidator(_repository);

            return validator.Validate(model);
        }

        public bool VerifyPermission(Guid actorId, string permissionName, string module = "Root")
        {
            var actor = _repository.GetActor(actorId);
            return actor.HasPermissionForModule(permissionName, module);
        }

        public IEnumerable<PermissionData> GetPermissions()
        {
            return _repository.Permissions.ToList().Select(p => new PermissionData
            {
                Id = p.Id,
                Module = p.Module,
                Name = p.Name
            });
        }

        public IEnumerable<PermissionData> GetActorPermissions(Guid actorId)
        {
            var actor = _repository.GetActor(actorId);
            return actor.GetPermissions().Select(p => new PermissionData
            {
                Id = p.Id,
                Module = p.Module,
                Name = p.Name
            });
        }

        public List<Guid> GetRolePermissions(Guid roleId)
        {
            return _repository.Roles.Single(r => r.Id == roleId).Permissions.Select(p => p.Id).ToList();
        }

        public ActorData GetActor(Guid actorId)
        {
            var actor = _repository.GetActor(actorId);

            return new ActorData
            {
                UserName = actor.Data.Username
            };
        }
    }
}
