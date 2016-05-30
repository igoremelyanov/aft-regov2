using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.Events
{
    public class DuplicateMechanismConfigurationCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }

        public DuplicateMechanismConfigurationCreated()
        {
            
        }

        public DuplicateMechanismConfigurationCreated(DuplicateMechanismConfiguration config)
        {
            Id = config.Id;
            BrandId = config.BrandId;
        }
    }
}
