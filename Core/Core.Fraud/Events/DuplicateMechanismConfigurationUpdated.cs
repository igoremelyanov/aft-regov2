using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.Events
{
    public class DuplicateMechanismConfigurationUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }

        public DuplicateMechanismConfigurationUpdated()
        {
            
        }

        public DuplicateMechanismConfigurationUpdated(DuplicateMechanismConfiguration configuration)
        {
            Id = configuration.Id;
            BrandId = configuration.BrandId;
        }
    }
}
