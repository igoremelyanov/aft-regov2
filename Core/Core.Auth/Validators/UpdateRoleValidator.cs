using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class UpdateRoleValidator: AbstractValidator<UpdateRole>
    {
        public UpdateRoleValidator(IAuthRepository repository)
        {
            RuleFor(m => m.RoleId)
                .Must(id => repository.Roles.Any(r => r.Id == id))
                .WithMessage(ErrorsCodes.RoleNotFound.ToString());

            RuleFor(m => m.Permissions)
                .Must(permissions => repository.Permissions.Count(p => permissions.Contains(p.Id)) == permissions.Count)
                .WithMessage(ErrorsCodes.PermissionIsNotRegistered.ToString());
        }
    }
}