using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class RejectPlayerBankAccountData
    {
        public Guid Id { get; set; }
    }

    public class RejectPlayerBankAccountValidator : AbstractValidator<RejectPlayerBankAccountData>
    {
        public RejectPlayerBankAccountValidator(IPaymentRepository repository)
        {
            PlayerBankAccount playerBankAccount = null;

            RuleFor(x => x.Id)
                .Must(x =>
                {
                    playerBankAccount = repository.PlayerBankAccounts.SingleOrDefault(y => y.Id == x);

                    return playerBankAccount != null;
                })
                .WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");

            When(x => playerBankAccount != null, () => RuleFor(x => x.Id)
                .Must(x => playerBankAccount.Status == BankAccountStatus.Pending)
                .WithMessage("{\"text\": \"app:payment.playerBankAccountNotPending\"}"));
        }
    }
}