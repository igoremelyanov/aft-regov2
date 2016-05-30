using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class AssignRoleValidator : AbstractValidator<AssignRole>
    {
        public AssignRoleValidator(IAuthRepository repository)
        {
            RuleFor(model => model.ActorId)
                .Must(actorId => repository.Actors.Any(x => x.Id == actorId))
                .WithMessage(ErrorsCodes.ActorDoesNotExist.ToString());

            RuleFor(model => model.RoleId)
                .Must(roleId => repository.Roles.Any(x => x.Id == roleId))
                .WithMessage(ErrorsCodes.RoleNotFound.ToString());
        }
    }
}