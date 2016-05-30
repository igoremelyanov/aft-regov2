using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Data;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Auth.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePassword>
    {
        public ChangePasswordValidator(IAuthRepository repository)
        {
            Func<Guid, Actor> actorGetter = id => repository.Actors.SingleOrDefault(a => a.Id == id);
            RuleFor(m => m.ActorId)
                .Must(actorId => actorGetter(actorId) != null)
                .WithMessage(ErrorsCodes.ActorDoesNotExist.ToString());
            RuleFor(m => m.NewPassword)
                .NotEmpty()
                .WithMessage(ErrorsCodes.PasswordIsEmpty.ToString());

            RuleFor(m => m.NewPassword)
                .Must((model, password) =>
                {
                    var actor = actorGetter(model.ActorId);

                    return actor.EncryptedPassword !=
                           new Entities.Actor(model.ActorId, actor.Username, password).Data.EncryptedPassword;
                })
                .WithMessage(ErrorsCodes.PasswordsMatch.ToString())
                .When(model => actorGetter(model.ActorId) != null);
        }
    }
}