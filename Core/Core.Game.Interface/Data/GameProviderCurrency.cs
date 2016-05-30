using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class GameProviderCurrency
    {
        public Guid Id { get; set; }

        public virtual GameProvider GameProvider { get; set; }
        public Guid GameProviderId { get; set; }

        public string CurrencyCode { get; set; }
        public string GameProviderCurrencyCode { get; set; }
    }
}
