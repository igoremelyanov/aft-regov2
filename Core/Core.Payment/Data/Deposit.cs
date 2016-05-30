using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Payment.Data
{  
    public class Deposit
    {        
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Licensee { get; set; }

        public Guid BrandId { get; set; }
        public RegoV2.Core.Payment.Data.Brand Brand { get; set; }

        public Guid PlayerId { get; set; }
        public Player Player { get; set; }
        
        [MaxLength(100)]
        public string ReferenceCode { get; set; }

        [MaxLength(100)]
        public string PaymentMethod { get; set; }
    
        [MaxLength(100)]
        public string CurrencyCode { get; set; }
        
        public decimal Amount { get; set; }

        public decimal UniqueDepositAmount { get; set; }
                
        public string Status { get; set; }
        
        public DateTimeOffset DateSubmitted { get; set; }

        public string SubmittedBy { get; set; }

        public DateTimeOffset? DateApproved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTimeOffset? DateVerified { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? DateUnverified { get; set; }

        public string UnverifiedBy { get; set; }

        public UnverifyReasons? UnverifyReason { get; set; }

        public DateTimeOffset? DateRejected { get; set; }

        public string RejectedBy { get; set; }

        public DepositType DepositType { get; set; }

        public Guid? BankAccountId { get; set; }
        public BankAccount BankAccount { get; set; }     
    }
}