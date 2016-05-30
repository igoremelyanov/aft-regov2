using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OfflineDepositConfirmValidator : AbstractValidator<OfflineDepositConfirm>
    {
        private readonly IPaymentRepository _repository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IPaymentQueries _paymentQueries;

        public OfflineDepositConfirmValidator(
            IPaymentRepository repository,
            IPlayerRepository playerRepository,
            IPaymentQueries paymentQueries)
        {
            _repository = repository;
            _playerRepository = playerRepository;
            _paymentQueries = paymentQueries;

            RuleFor(x=>x.PlayerAccountName)
                .NotEmpty()
                .WithMessage("FieldIsRequired");

            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("AmountGreaterZero");

            RuleFor(x => x)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .Must(x =>
                {
                    var deposit = _repository.OfflineDeposits.Single(o => o.Id == x.Id);

                    var player =
                        _playerRepository.Players.Include(y => y.IdentityVerifications)
                            .Single(y => y.Id == deposit.PlayerId);

                    return IsAccountNameValid(x.PlayerAccountName, player)
                           || (!string.IsNullOrEmpty(x.IdFrontImage) && !string.IsNullOrEmpty(x.IdBackImage))
                           || !string.IsNullOrEmpty(x.ReceiptImage);
                })
                .WithMessage("FrontAndBackCopyRequired")
                .WithName("IdFront")
                .Must(x =>
                {
                    var deposit = _repository.OfflineDeposits.Single(o => o.Id == x.Id);

                    var player =
                        _playerRepository.Players.Include(y => y.IdentityVerifications)
                            .Single(y => y.Id == deposit.PlayerId);

                    return player.IdentityVerifications
                        .Any(
                            o => o.ExpirationDate >= DateTime.Now && o.VerificationStatus == VerificationStatus.Verified)
                           || (!string.IsNullOrEmpty(x.IdFrontImage) && !string.IsNullOrEmpty(x.IdBackImage))
                           || !string.IsNullOrEmpty(x.ReceiptImage);
                })
                .WithMessage("NotActiveIdentificationDocs")
                .WithName("IdFront")
                .Must(x =>
                {
                    var bankAccount = _paymentQueries.GetBankAccountForOfflineDeposit(x.Id);
                    var isReceiptRequired = IsReceiptRequiredByPaymentLevelCriterium(x, bankAccount);

                    return !isReceiptRequired || !string.IsNullOrEmpty(x.ReceiptImage);
                })
                .WithMessage("ReceiptRequired")
                .WithName("DepositReceipt");
        }

        private static bool IsReceiptRequiredByPaymentLevelCriterium(OfflineDepositConfirm deposit, BankAccount bankAccount)
        {
            if (deposit.TransferType == TransferType.DifferentBank)
            {
                switch (deposit.OfflineDepositType)
                {
                    case DepositMethod.ATM:
                        return bankAccount.AtmDifferentBank;
                    case DepositMethod.CounterDeposit:
                        return bankAccount.CounterDepositDifferentBank;
                    case DepositMethod.InternetBanking:
                        return bankAccount.InternetDifferentBank;
                    default:
                        return false;
                }
            }

            if (deposit.TransferType == TransferType.SameBank)
            {
                switch (deposit.OfflineDepositType)
                {
                    case DepositMethod.ATM:
                        return bankAccount.AtmSameBank;
                    case DepositMethod.CounterDeposit:
                        return bankAccount.CounterDepositSameBank;
                    case DepositMethod.InternetBanking:
                        return bankAccount.InternetSameBank;
                    default:
                        return false;
                }
            }

            return false;
        }

        public static bool IsAccountNameValid(string playerAccountName, Common.Data.Player.Player player)
        {
            return string.Equals(playerAccountName, player.FirstName + " " + player.LastName, StringComparison.InvariantCultureIgnoreCase) || string.Equals(playerAccountName, player.LastName + " " + player.FirstName, StringComparison.InvariantCultureIgnoreCase) || string.Equals(playerAccountName, player.FirstName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
