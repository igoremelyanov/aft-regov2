using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class GameProviderCurrencyCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid GameProviderId { get; set; }
        public string CurrencyCode { get; set; }
        public string GameProviderCurrencyCode { get; set; }

        public GameProviderCurrencyCreated()
        {
        }

        public GameProviderCurrencyCreated(GameProviderCurrency gameProviderCurrency)
        {
            Id = gameProviderCurrency.Id;
            GameProviderId = gameProviderCurrency.GameProviderId;
            CurrencyCode = gameProviderCurrency.CurrencyCode;
            GameProviderCurrencyCode = gameProviderCurrency.GameProviderCurrencyCode;
        }
    }
}
