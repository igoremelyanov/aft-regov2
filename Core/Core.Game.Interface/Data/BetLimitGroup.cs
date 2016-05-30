using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class BetLimitGroup
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public int ExternalId { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}
