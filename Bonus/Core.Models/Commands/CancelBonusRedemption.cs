using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CancelBonusRedemption
    {
        public Guid PlayerId { get; set; }
        public Guid RedemptionId { get; set; }
    }
}
