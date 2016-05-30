using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class BankAccount
    {       
        public Guid Id { get; set; }

        public string AccountId { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public BankAccountType AccountType { get; set; }

        public string Province { get; set; }

        public string Branch { get; set; }

        public string CurrencyCode { get; set; }

        public Bank Bank { get; set; }

        public string SupplierName { get; set; }

        public string ContactNumber { get; set; }

        public string USBCode { get; set; }

        public DateTime? PurchasedDate { get; set; }

        public DateTime? UtilizationDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public Guid? IdFrontImage { get; set; }

        public Guid? IdBackImage { get; set; }

        public Guid? ATMCardImage { get; set; }

        public string Remarks { get; set; }

        public BankAccountStatus Status { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset Created { get; set; }

        public string UpdatedBy { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public virtual ICollection<PaymentLevel> PaymentLevels { get; set; }

        public bool InternetSameBank { get; set; }

        public bool AtmSameBank { get; set; }

        public bool CounterDepositSameBank { get; set; }

        public bool InternetDifferentBank { get; set; }

        public bool AtmDifferentBank { get; set; }

        public bool CounterDepositDifferentBank { get; set; }
    }
}