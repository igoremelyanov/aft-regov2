using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class PlayerBankAccountId
    {
        private readonly Guid _id;

        public PlayerBankAccountId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(PlayerBankAccountId id)
        {
            return id._id;
        }

        public static implicit operator PlayerBankAccountId(Guid id)
        {
            return new PlayerBankAccountId(id);
        }
    }

    public class PlayerBankAccount
    {
        public Guid Id { get; set; }
                    
        public Player Player { get; set; }
        
        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string Province { get; set; }

        public string City { get; set; }

        public string Branch { get; set; }

        public string SwiftCode { get; set; }

        public string Address { get; set; }

        public Bank Bank { get; set; }

        public string Remarks { get; set; }

        public BankAccountStatus Status { get; set; }

        public bool EditLock { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset Created { get; set; }

        public string UpdatedBy { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? Verified { get; set; }

        public string RejectedBy { get; set; }

        public DateTimeOffset? Rejected { get; set; }

        public bool IsCurrent { get; set; }
    }
}