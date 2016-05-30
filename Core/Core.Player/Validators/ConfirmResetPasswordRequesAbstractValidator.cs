using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class ConfirmResetPasswordRequesAbstractValidator : AbstractValidator<ConfirmResetPasswordData>
    {
        public ConfirmResetPasswordRequesAbstractValidator(IPlayerRepository playerRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(o => o)
                .NotEmpty()
                .WithMessage("FieldIsRequired")
                .Must(o => string.Equals(o.ConfirmPassword, o.NewPassword, StringComparison.OrdinalIgnoreCase))
                .WithMessage(PlayerAccountResponseCode.PasswordsCombinationIsNotValid.ToString())
                .WithName("ConfirmPassword");

            RuleFor(p => p.NewPassword)
                .NotEmpty()
                .WithMessage(PlayerAccountResponseCode.PasswordShouldNotBeEmpty.ToString())
                .Length(6, 12)
                .WithMessage(PlayerAccountResponseCode.PasswordIsNotWithinItsAllowedRange.ToString())
                .WithName("ConfirmPassword");

            When(x => x.PlayerId != Guid.Empty && !string.IsNullOrWhiteSpace(x.NewPassword),
                () => RuleFor(changePasswordRequest => changePasswordRequest.PlayerId)
                .Must(playerId => playerRepository.Players.SingleOrDefault(x => x.Id == playerId) != null)
                .WithMessage(PlayerAccountResponseCode.PlayerDoesNotExist.ToString())
                .WithName("ConfirmPassword"));
        }
    }
}
