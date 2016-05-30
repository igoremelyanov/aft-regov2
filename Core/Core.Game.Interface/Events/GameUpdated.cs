using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Game.Interface.Events
{
    public class GameUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid GameProviderId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
