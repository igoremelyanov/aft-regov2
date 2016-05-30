using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class OfflineWithdrawId
    {
        private readonly Guid _id;

        public OfflineWithdrawId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(OfflineWithdrawId id)
        {
            return id._id;
        }

        public static implicit operator OfflineWithdrawId(Guid id)
        {
            return new OfflineWithdrawId(id);
        }
    }

    public class OfflineWithdraw
    {
        public Guid Id { get; set; }

        public PlayerBankAccount PlayerBankAccount { get; set; }

        public string TransactionNumber { get; set; }
        
        public decimal Amount { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? Verified { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? Unverified { get; set; }

        public string UnverifiedBy { get; set; }

        public DateTimeOffset? Approved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTimeOffset? Rejected { get; set; }

        public string RejectedBy { get; set; }
        
        public string Remarks { get; set; }

        public WithdrawalStatus Status { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public CommonVerificationStatus? DocumentsCheckStatus { get; set; }
        public CommonVerificationStatus? InvestigationStatus { get; set; }
        public CommonVerificationStatus? AutoVerificationCheckStatus { get; set; }

        public DateTimeOffset? AutoVerificationCheckDate { get; set; }
        public DateTimeOffset? DocumentsCheckDate { get; set; }
        public DateTimeOffset? InvestigationDate { get; set; }

        public FraudRiskLevelStatus? RiskLevelStatus { get; set; }
        public DateTimeOffset? RiskLevelCheckDate { get; set; }

        public bool Exempted { get; set; }

        public DateTimeOffset? ExemptionCheckTime { get; set; }

        public bool AutoWagerCheck { get; set; }

        public DateTimeOffset? AutoWagerCheckTime { get; set; }

        public bool WagerCheck { get; set; }

        //public virtual ICollection<OfflineWithdrawalHistory> OfflineWithdrawalHistory { get; set; }

        public string AcceptedBy { get; set; }

        public DateTimeOffset? AcceptedTime { get; set; }

        public string RevertedBy { get; set; }

        public DateTimeOffset? RevertedTime { get; set; }

        public string CanceledBy { get; set; }
        public string InvestigatedBy { get; set; }
        public DateTimeOffset? CanceledTime { get; set; }
    }

    public enum CommonVerificationStatus
    {
        Failed,
        Passed
    }
}