using System;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class Lock: Identity
    {
        public decimal Amount { get; set; }
        public Guid RedemptionId { get; set; }

        public DateTimeOffset LockedOn { get; set; }
        public DateTimeOffset? UnlockedOn { get; set; }
    }
}
