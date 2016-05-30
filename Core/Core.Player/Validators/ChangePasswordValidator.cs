using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordData>
    {
        public ChangePasswordValidator(IPlayerRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(p => p.Username)
                .NotEmpty()
                .WithMessage(PlayerAccountResponseCode.UsernameShouldNotBeEmpty.ToString());

            RuleFor(p => p.NewPassword)
                .NotEmpty()
                .WithMessage(PlayerAccountResponseCode.PasswordShouldNotBeEmpty.ToString())
                .Length(6, 12)
                .WithMessage(PlayerAccountResponseCode.PasswordIsNotWithinItsAllowedRange.ToString());

            When(x => !string.IsNullOrWhiteSpace(x.Username) && !string.IsNullOrWhiteSpace(x.NewPassword), () => RuleFor(changePasswordRequest => changePasswordRequest.Username)
                .Must(username => repository.Players.SingleOrDefault(x => x.Username == username) != null)
                .WithMessage(PlayerAccountResponseCode.PlayerDoesNotExist.ToString()));
        }
    }
}