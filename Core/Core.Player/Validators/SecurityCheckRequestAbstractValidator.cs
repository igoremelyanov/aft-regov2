using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class SecurityCheckRequestAbstractValidator : AbstractValidator<SecurityAnswerData>
    {
        public SecurityCheckRequestAbstractValidator(IPlayerQueries playerQueries)
        {
            RuleFor(o => o.Answer)
                .NotEmpty()
                .WithMessage("FieldIsRequired");

            RuleFor(o => o.PlayerId)
                .Must(id => playerQueries.GetPlayer(id) != null)
                .WithMessage(PlayerAccountResponseCode.PlayerDoesNotExist.ToString())
                .WithName("Answer");

            RuleFor(p => p)
                .Must(data => string.Compare(playerQueries.GetPlayer(data.PlayerId).SecurityAnswer, data.Answer, StringComparison.OrdinalIgnoreCase) == 0)
                .WithMessage(PlayerAccountResponseCode.IncorrectSecurityAnswer.ToString())
                .WithName("Answer");
        }
    }
}
