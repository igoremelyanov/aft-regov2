using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Security.Events
{
    public class AdminIpRegulationDeleted : DomainEventBase
    {
        public AdminIpRegulationDeleted() { } // default constructor is required for publishing event to MQ

        public AdminIpRegulationDeleted(AdminIpRegulation regulation)
        {
            Id = regulation.Id;
        }

        public Guid Id { get; set; }

    }
}
