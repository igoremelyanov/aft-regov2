using System;
using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class PaymentLevelSaveResult
    {
        public string Message { get; set; }
        public Guid PaymentLevelId { get; set; }
    }

    public class EditPaymentLevel
    {
        public Guid? Id { get; set; }
        public Guid? Brand { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool EnableOfflineDeposit { get; set; }
        public bool EnableOnlineDeposit { get; set; }
        public bool IsDefault { get; set; }
        public string[] BankAccounts { get; set; }
        public string[] PaymentGatewaySettings { get; set; }
        public decimal MaxBankFee { get; set; }
        public decimal BankFeeRatio { get; set; }

        public Guid[] InternetSameBankSelection { get; set; }
        public Guid[] AtmSameBankSelection { get; set; }
        public Guid[] CounterDepositSameBankSelection { get; set; }

        public Guid[] InternetDifferentBankSelection { get; set; }
        public Guid[] AtmDifferentBankSelection { get; set; }
        public Guid[] CounterDepositDifferentBankSelection { get; set; }

        public EditPaymentLevel()
        {
            InternetSameBankSelection = Enumerable.Empty<Guid>().ToArray();
            AtmSameBankSelection = Enumerable.Empty<Guid>().ToArray();
            CounterDepositSameBankSelection = Enumerable.Empty<Guid>().ToArray();

            InternetDifferentBankSelection = Enumerable.Empty<Guid>().ToArray();
            AtmDifferentBankSelection = Enumerable.Empty<Guid>().ToArray();
            CounterDepositDifferentBankSelection = Enumerable.Empty<Guid>().ToArray();
        }
    }

    public class ActivatePaymentLevelCommand
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class DeactivatePaymentLevelCommand
    {
        public Guid Id { get; set; }
        public Guid? NewPaymentLevelId { get; set; }
        public string Remarks { get; set; }
    }

    public class PaymentLevelTransferObj
    {
        public object Brand { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool EnableOfflineDeposit { get; set; }
        public bool EnableOnlineDeposit { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<Guid> BankAccounts { get; set; }
        public IEnumerable<Guid> PaymentGatewaySettings { get; set; }
        public decimal MaxBankFee { get; set; }
        public decimal BankFeeRatio { get; set; }
    }
    public enum DeactivatePaymentLevelStatus
    {
        CanDeactivate,
        CanDeactivateIsDefault,
        CanDeactivateIsAssigned,
        CannotDeactivateNoReplacement,
        CannotDeactivateStatusInactive
    }
}
