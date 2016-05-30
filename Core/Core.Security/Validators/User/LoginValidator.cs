using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.Data.Users;
using FluentValidation;

namespace AFT.RegoV2.Core.Security.Validators.User
{
    public class LoginValidator : AbstractValidator<Admin>
    {
        public LoginValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(admin => admin)
                .NotNull()
                .WithMessage(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString())
                .WithName("Admin");

            When(admin => admin != null, () =>
            {
                RuleFor(admin => admin.IsActive)
                    .NotEqual(false)
                    .WithMessage(PlayerAccountResponseCode.NonActive.ToString());
            });
        }
    }
}