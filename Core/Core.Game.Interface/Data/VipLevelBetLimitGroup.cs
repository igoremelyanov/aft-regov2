using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class VipLevelBetLimitGroup
    {
        public Guid VipLevelId { get; set; }
        public Guid BetLimitGroupId { get; set; }
        
        public virtual VipLevel VipLevel { get; set; }
        public virtual BetLimitGroup BetLimitGroup { get; set; }
    }
}