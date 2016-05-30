using System;
using System.Threading;
using AFT.RegoV2.Core.Common.Utils;
using Newtonsoft.Json;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public abstract class DomainEventBase : IDomainEvent
    {
        protected DomainEventBase()
        {
            EventId = Identifier.NewSequentialGuid();
            EventCreated = Identifier.NewDateTimeOffset().LocalDateTime;
            EventCreatedBy = Thread.CurrentPrincipal.Identity.Name;
        }

        [JsonProperty]
        public Guid EventId { get; }
        [JsonProperty]
        public Guid AggregateId { get; set; }
        [JsonProperty]
        public DateTimeOffset EventCreated { get; set; }
        [JsonProperty]
        public string EventCreatedBy { get; set; }
    }
}