using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class WithdrawalLock
    {
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }

        public Guid WithdrawalId { get; set; }

        public decimal Amount { get; set; }

        public Status Status { get; set; }

        public DateTimeOffset LockedOn { get; set; }

        public string LockedBy { get; set; }

        public DateTimeOffset? UnLockedOn { get; set; }

        public string UnLockedBy { get; set; }
    }
}