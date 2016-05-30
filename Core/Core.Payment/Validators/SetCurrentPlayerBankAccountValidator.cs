using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class SetCurrentPlayerBankAccountValidator : AbstractValidator<SetCurrentPlayerBankAccountData>
    {
        public SetCurrentPlayerBankAccountValidator(IPaymentRepository repository)
        {
            RuleFor(x => x.PlayerBankAccountId)
                .Must(x => repository.PlayerBankAccounts.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");
        }
    }
}