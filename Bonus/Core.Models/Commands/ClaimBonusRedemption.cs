using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class ClaimBonusRedemption
    {
        public Guid PlayerId { get; set; }
        public Guid RedemptionId { get; set; }
    }
}
