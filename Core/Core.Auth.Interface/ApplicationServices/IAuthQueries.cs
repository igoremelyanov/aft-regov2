using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Auth.Interface.ApplicationServices
{
    public interface IAuthQueries
    {
        ValidationResult GetValidationResult(LoginActor model);
        ValidationResult GetValidationResult(CreateActor model);
        ValidationResult GetValidationResult(CreatePermission model);
        ValidationResult GetValidationResult(ChangePassword model);
        ValidationResult GetValidationResult(CreateRole model);
        ValidationResult GetValidationResult(UpdateRole model);
        ValidationResult GetValidationResult(AssignRole model);
        bool VerifyPermission(Guid actorId, string permissionName, string module = "Root");
        List<Guid> GetRolePermissions(Guid roleId);
        IEnumerable<PermissionData> GetPermissions();
        IEnumerable<PermissionData> GetActorPermissions(Guid actorId);
        ActorData GetActor(Guid actorId);
    }
}