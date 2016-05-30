using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class CreatePermissionValidator: AbstractValidator<CreatePermission>
    {
        public CreatePermissionValidator(IAuthRepository authRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(m => m)
                .Must(m => string.IsNullOrWhiteSpace(m.Name) == false && string.IsNullOrWhiteSpace(m.Module) == false)
                .WithMessage(ErrorsCodes.ModuleOrPermissionNameIsEmpty.ToString())
                .Must(m => authRepository.Permissions.Any(p => p.Name == m.Name && p.Module == m.Module) == false)
                .WithMessage(ErrorsCodes.DuplicatePermission.ToString())
                .WithName("Model");
        }
    }
}
