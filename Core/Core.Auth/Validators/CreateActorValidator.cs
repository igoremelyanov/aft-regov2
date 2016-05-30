using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class CreateActorValidator : AbstractValidator<CreateActor>
    {
        public CreateActorValidator(IAuthRepository repository)
        {
            RuleFor(model => model.ActorId)
                .Must(actorId =>
                {
                    return repository.Actors.Any(x => x.Id == actorId) == false;
                })
                .WithMessage(ErrorsCodes.ActorAlreadyCreated.ToString());

            RuleFor(model => model.Username)
                .NotEmpty()
                .WithMessage(ErrorsCodes.UsernameIsEmpty.ToString());

            RuleFor(model => model.Password)
                .NotEmpty()
                .WithMessage(ErrorsCodes.PasswordIsEmpty.ToString());
        }
    }
}