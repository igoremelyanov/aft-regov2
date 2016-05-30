using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{  
    public class DepositDto
    {        
        public Guid Id { get; set; }

        public string Licensee { get; set; }

        public Guid BrandId { get; set; }

        public string BrandName { get; set; }

        public Guid PlayerId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
                
        public string ReferenceCode { get; set; }
        
        public string PaymentMethod { get; set; }
    
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

        public DateTimeOffset? DateRejected { get; set; }

        public string RejectedBy { get; set; }
        
        public DepositType DepositType { get; set; }
        
        public string BankAccountId { get; set; }

        public string BankName { get; set; }
        
        public string BankProvince { get; set; }

        public string BankBranch { get; set; }

        public string BankAccountName { get; set; }

        public string BankAccountNumber { get; set; }

        public string PlayerName
        {
            get { return FirstName + " " + LastName; }
        }
        public string BonusCode { get; set; }

        public Guid? BonusId { get; set; }
        public Guid? BonusRedemptionId { get; set; }
        public UnverifyReasons? UnverifyReason { get; set; }
    }
}