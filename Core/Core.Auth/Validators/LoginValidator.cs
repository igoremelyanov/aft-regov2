using System.Linq;
using AFT.RegoV2.Core.Auth.Entities;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class LoginValidator : AbstractValidator<LoginActor>
    {
        public LoginValidator(IAuthRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(actor => actor)
                .Must(model =>
                {
                    return repository.Actors.Any(x => x.Id == model.ActorId);
                })
                .WithMessage(ErrorsCodes.ActorDoesNotExist.ToString())
                .Must(model =>
                {
                    var actor = repository.GetActor(model.ActorId);

                    return actor.Data.EncryptedPassword == new Actor(model.ActorId, actor.Data.Username, model.Password).Data.EncryptedPassword;
                })
                .WithMessage(ErrorsCodes.ActorPasswordIsNotValid.ToString())
                .WithName("Actor");
        }
    }
}