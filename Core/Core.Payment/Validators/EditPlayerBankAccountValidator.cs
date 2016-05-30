using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class EditPlayerBankAccountValidator : AbstractValidator<EditPlayerBankAccountData>
    {
        public EditPlayerBankAccountValidator(IPaymentRepository repository, IPaymentQueries queries)
        {
            RuleFor(x => x.Id)
                .Must(x => repository.PlayerBankAccounts.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");

            RuleFor(x => x.PlayerId)
                .Must(x => x != null && x.Value != Guid.Empty && queries.GetPlayer(x.Value) != null)
                .WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");

            RuleFor(x => x.Bank)
                .Must(x => repository.Banks.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");

            RuleFor(x => x.Province)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.City)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.AccountName)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.AccountNumber)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
        }
    }
}