using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OfflineDepositValidator: AbstractValidator<OfflineDepositRequest>
    {
        public OfflineDepositValidator(
            IPaymentRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Func<Guid, Data.Player> playerGetter = playerId => repository.Players.SingleOrDefault(p => p.Id == playerId);
            Func<Guid, BankAccount> bankAccountGetter = bankAccountId => repository.BankAccounts
                .Include(x => x.Bank)
                .SingleOrDefault(x => x.Id == bankAccountId && x.Status == BankAccountStatus.Active);

            RuleFor(od => od.PlayerId)
                .Must(id => playerGetter(id) != null)
                .WithMessage("Player not found");

            RuleFor(od => od.BankAccountId)
                .Must(id => bankAccountGetter(id) != null)
                .WithMessage("app:payment.deposit.bankAccountNotFound")
                .Must((request, guid) =>
                {
                    var player = playerGetter(request.PlayerId);
                    var bankAccount = bankAccountGetter(request.BankAccountId);

                    return bankAccount.CurrencyCode == player.CurrencyCode;
                })
                .WithMessage("app:payment.deposit.differentCurrenciesErrorMessage")
                .When(od => playerGetter(od.PlayerId) != null);

            RuleFor(od => od.Amount)
                .Must(amount => amount > 0)
                .WithMessage("Deposit amount should be greater than 0");

            RuleFor(x => x.Amount)
                .Must(amount => amount > 0)
                .WithMessage("Amount has to be greater than zero.");
        }
    }
}
