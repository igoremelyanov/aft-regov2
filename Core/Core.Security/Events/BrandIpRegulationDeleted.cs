using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data.IpRegulations;

namespace AFT.RegoV2.Core.Security.Events
{
    public class BrandIpRegulationDeleted : DomainEventBase
    {
        public BrandIpRegulationDeleted() { } // default constructor is required for publishing event to MQ

        public BrandIpRegulationDeleted(BrandIpRegulation regulation)
        {
            Id = regulation.Id;
        }

        public Guid Id { get; set; }

    }
}
