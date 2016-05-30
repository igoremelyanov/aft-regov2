using System;
using AFT.RegoV2.Core.Common.Interfaces;
using Newtonsoft.Json;

namespace AFT.RegoV2.Core.Game.Interface.Events
{
    public abstract class GameActionEventBase : DomainEventBase
    {
        [JsonProperty]
        public Guid PlayerId { get; set; }
        [JsonProperty]
        public Guid BrandId { get; set; }
        [JsonProperty]
        public Guid GameId { get; set; }
        [JsonProperty]
        public Guid RoundId { get; set; }
        [JsonProperty]
        public Guid GameActionId { get; set; }
        [JsonProperty]
        public Guid? RelatedGameActionId { get; set; }
        [JsonProperty]
        public DateTimeOffset CreatedOn { get; set; }

        public decimal Amount { get; set; }

        public decimal Turnover { get; set; }
        public decimal Ggr { get; set; }
        public decimal UnsettledBets { get; set; }
    }
}