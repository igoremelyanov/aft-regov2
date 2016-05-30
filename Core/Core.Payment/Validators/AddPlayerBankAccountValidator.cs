using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class AddPlayerBankAccountValidator : AbstractValidator<EditPlayerBankAccountData>
    {
        public AddPlayerBankAccountValidator(IPaymentRepository repository, IPaymentQueries queries)
        {
            RuleFor(x => x.AccountNumber)
                .Must(
                    (editPlayerBankAccountData, accountNumber) =>
                        repository.PlayerBankAccounts.Count(ba =>
                            ba.AccountNumber == accountNumber &&
                            ba.Bank.Id == editPlayerBankAccountData.Bank) == 0)
                .WithMessage("{\"text\": \"app:common.similarBankAccountNumberAlreadyExists\"}");

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

            RuleFor(x => x)
		        .Must(x =>
		        {
			        var player = repository.Players.Single(p => p.Id == x.PlayerId);
			        return player.GetFullName() == x.AccountName;
		        })
		        .WithName("AccountName")
		        .WithMessage("{\"text\": \"app:common.invalidAccountName\"}");
        }
    }
}