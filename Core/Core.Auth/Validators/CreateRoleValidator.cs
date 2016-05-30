using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class CreateRoleValidator: AbstractValidator<CreateRole>
    {
        public CreateRoleValidator(IAuthRepository repository)
        {
            RuleFor(m => m.RoleId)
                .Must(id => repository.Roles.Any(r => r.Id == id) == false)
                .WithMessage(ErrorsCodes.RoleAlreadyCreated.ToString());

            RuleFor(m => m.Permissions)
                .Must(permissions => repository.Permissions.Count(p => permissions.Contains(p.Id)) == permissions.Count)
                .WithMessage(ErrorsCodes.PermissionIsNotRegistered.ToString());
        }
    }
}