using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Data
{  
    public class BankAccount
    {
        public BankAccount()
        {
            PaymentLevels = new List<PaymentLevel>();
        }

        public Guid Id { get; set; }
            
        [Required, MinLength(1), MaxLength(20)]
        public string AccountId { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public string AccountName { get; set; }

        [Required, MinLength(1), MaxLength(50)]
        public string AccountNumber { get; set; }

        [Required]
        public BankAccountType AccountType { get; set; }

        [Required, MinLength(1), MaxLength(200)]
        public string Province { get; set; }

        [Required, MinLength(1), MaxLength(200)]
        public string Branch { get; set; }

        [Required]
        public string CurrencyCode { get; set; }

        [Required]
        public Bank Bank { get; set; }

        [Required, MinLength(1), MaxLength(50)]
        public string SupplierName { get; set; }

        [Required, MinLength(1), MaxLength(20)]
        public string ContactNumber { get; set; }

        [Required, MinLength(1), MaxLength(20)]
        public string USBCode { get; set; }

        public DateTime? PurchasedDate { get; set; }

        public DateTime? UtilizationDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public Guid? IdFrontImage { get; set; }

        public Guid? IdBackImage { get; set; }

        public Guid? ATMCardImage { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Remarks { get; set; }

        [Required]
        public BankAccountStatus Status { get; set; }

        [Required]
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