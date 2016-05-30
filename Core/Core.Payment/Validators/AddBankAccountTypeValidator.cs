using AFT.RegoV2.Core.Payment.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class AddBankAccountTypeValidator : AbstractValidator<BankAccountType>
    {
        public AddBankAccountTypeValidator(IPaymentRepository paymentRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
        }
    }
}
