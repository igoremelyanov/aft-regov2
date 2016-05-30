using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class BrandGameProviderConfiguration
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        public Guid GameProviderId { get; set; }
        public GameProvider GameProvider { get; set; }

        public Guid GameProviderConfigurationId { get; set; }
        public GameProviderConfiguration GameProviderConfiguration { get; set; }
    }
}
