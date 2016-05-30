using System;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Interface.Events
{
    public class BrandCredentialsUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public BrandCredentialsUpdated()
        {
        }
    }
}
