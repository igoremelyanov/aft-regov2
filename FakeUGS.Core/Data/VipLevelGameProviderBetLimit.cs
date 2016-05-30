using System;

namespace FakeUGS.Core.Data
{
    public class VipLevelGameProviderBetLimit
    {
        public Guid     VipLevelId { get; set; }
        public Guid     BetLimitId { get; set; }

        public Guid     GameProviderId { get; set; }
        public string   CurrencyCode { get; set; }

        public VipLevel     VipLevel { get; set; }
        public GameProviderBetLimit     BetLimit { get; set; }
        public GameProvider   GameProvider { get; set; }
    }
}