using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class ResetPasswordRequestAbstractValidator : AbstractValidator<ResetPasswordData>
    {
        public ResetPasswordRequestAbstractValidator(IPlayerQueries playerQueries)
        {
            RuleFor(o => o.Id)
                .NotEmpty()
                .WithMessage("FieldIsRequired")
                .Must(o =>
                {
                    var player = playerQueries.GetPlayerByUsername(o) ?? playerQueries.GetPlayerByEmail(o);

                    return player != null;
                })
                .WithMessage("PlayerDoesNotExist");
        }
    }
}
