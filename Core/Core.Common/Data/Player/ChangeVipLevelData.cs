using System;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class ChangeVipLevelData
    {
        public Guid PlayerId { get; set; }
        public Guid NewVipLevel { get; set; }
        public string Remarks { get; set; }
    }
}