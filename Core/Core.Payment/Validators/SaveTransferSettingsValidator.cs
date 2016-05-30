using System.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    class SaveTransferSettingsValidator : AbstractValidator<SaveTransferSettingsCommand>
    {
        public SaveTransferSettingsValidator()
        {
            //R1.0 supports only single wallet,  
            //for that reason there is no need to manage transfers
            //from one wallet to another - 14-Apr-2016,Vlad.S.
            //Rule for blocking settings creation
            RuleFor(x => x)
                .Must(x => x != null)
                .WithMessage(TransferFundSettingsErrors.NotImplementedError.ToString());

            RuleFor(x => x.MinAmountPerTransaction)
                .Must(x => x >= 0)
                .WithMessage(TransferFundSettingsErrors.MinAmountPerTransactionError.ToString());

            RuleFor(x => x.MaxAmountPerTransaction)
                .Must(x => x >= 0)
                .WithMessage(TransferFundSettingsErrors.MaxAmountPerTransactionError.ToString());

            RuleFor(x => x.MaxAmountPerTransaction)
                .Must((data, x) =>
                {
                    if (data.MinAmountPerTransaction != 0 && data.MaxAmountPerTransaction != 0)
                        return x > data.MinAmountPerTransaction;

                    return true;
                })
                .WithMessage(TransferFundSettingsErrors.MaxminAmountPerTransactionError.ToString());

            RuleFor(x => x.MaxAmountPerDay)
                .Must(x => x >= 0)
                .WithMessage(TransferFundSettingsErrors.MaxAmountPerDayError.ToString());
            
            RuleFor(x => x.MaxAmountPerDay)
                .Must((data, x) =>
                {
                    if (data.MinAmountPerTransaction != 0 && data.MaxAmountPerDay != 0)
                        return x >= data.MinAmountPerTransaction;

                    return true;
                })
                .WithMessage(TransferFundSettingsErrors.MinAmountPerTransactionErrorAmountPerDay.ToString());

            RuleFor(x => x.MaxAmountPerTransaction)
               .Must((data, x) =>
               {
                   if (data.MaxAmountPerTransaction != 0 && data.MaxAmountPerDay != 0)
                       return x <= data.MaxAmountPerDay;

                   return true;
               })
               .WithMessage(TransferFundSettingsErrors.MaxAmountPerTransactionErrorAmountPerDay.ToString());

            RuleFor(x => x.MaxTransactionPerDay)
                .Must(x => x >= 0)
                .WithMessage(TransferFundSettingsErrors.MaxTransactionPerDayError.ToString());

            RuleFor(x => x.MaxTransactionPerWeek)
                .Must(x => x >= 0)
                .WithMessage(TransferFundSettingsErrors.MaxTransactionPerWeekError.ToString());

            RuleFor(x => x.MaxTransactionPerWeek)
                .Must((data, x) =>
                {
                    if (data.MaxTransactionPerWeek != 0 && data.MaxTransactionPerDay != 0)
                        return x >= data.MaxTransactionPerDay;

                    return true;
                })
                .WithMessage(TransferFundSettingsErrors.MaxTransactionPerWeekErrorPerDay.ToString());

            RuleFor(x => x.MaxTransactionPerMonth)
               .Must(x => x >= 0)
               .WithMessage(TransferFundSettingsErrors.MaxTransactionPerMonthError.ToString());

            RuleFor(x => x.MaxTransactionPerMonth)
                .Must((data, x) =>
                {
                    if (data.MaxTransactionPerWeek != 0 && data.MaxTransactionPerMonth != 0)
                        return x >= data.MaxTransactionPerWeek;

                    return true;
                })
                .WithMessage(TransferFundSettingsErrors.MaxTransactionPerMonthErrorPerWeek.ToString());

            RuleFor(x => x.MaxTransactionPerMonth)
                .Must((data, x) =>
                {
                    if (data.MaxTransactionPerDay != 0 && data.MaxTransactionPerMonth != 0)
                        return x >= data.MaxTransactionPerDay;

                    return true;
                })
                .WithMessage(TransferFundSettingsErrors.MaxTransactionPerMonthErrorPerDay.ToString());
        }
    }
}
