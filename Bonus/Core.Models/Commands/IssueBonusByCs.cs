using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class IssueBonusByCs
    {
        public Guid PlayerId { get; set; }
        public Guid BonusId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
