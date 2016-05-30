using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player.Resources;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class SendNewPasswordValidator : AbstractValidator<SendNewPasswordData>
    {
        public SendNewPasswordValidator(IPlayerRepository repository, IAuthQueries authQueries)
        {
            int passwordMinLength = CommonPlayerSettings.PasswordMinLength;
            int passwordMaxLength = CommonPlayerSettings.PasswordMaxLength;

            string passwordIsNotWithinItsAllowedRangeErrorMessage =
                string.Format(Messages.PasswordIsNotWithinItsAllowedRangeErrorMessageFormat, passwordMinLength,
                    passwordMaxLength);

            RuleFor(x => x.PlayerId)
                .Must(x => repository.Players.Any(p => p.Id == x))
                .WithMessage(Messages.InvalidPlayerId)
                .DependentRules(x =>
                {
                    RuleFor(y => y.NewPassword)
                        .Length(passwordMinLength, passwordMaxLength)
                        .WithMessage(passwordIsNotWithinItsAllowedRangeErrorMessage)
                        .Must((data, y) =>
                        {
                            var loginValidationResult = authQueries.GetValidationResult(new LoginActor
                            {
                                ActorId = data.PlayerId,
                                Password = data.NewPassword
                            });

                            var passwordsMatch = loginValidationResult.IsValid;

                            return !passwordsMatch;
                        })
                        .WithMessage(Messages.PasswordsMatch);
                });

            
        }
    }

    public static class PasswordGenerator
    {
        public static string Create()
        {
            var passwordMinLength = CommonPlayerSettings.PasswordMinLength;
            var passwordMaxLength = CommonPlayerSettings.PasswordMaxLength;
            var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            var r = new Random();
            var passwordLength = r.Next(passwordMinLength, passwordMaxLength);
            var pass = new char[passwordLength];
            for (var i = 0; i < passwordLength; i++)
            {
                pass[i] = alpha[r.Next(alpha.Length - 1)];
            }
            return new string(pass);
        }
    }
}