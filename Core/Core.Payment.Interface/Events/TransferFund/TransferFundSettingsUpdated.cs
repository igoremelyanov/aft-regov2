using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class TransferFundSettingsUpdated : DomainEventBase
    {
        public TransferFundSettingsUpdated() { } // default constructor is required for publishing event to MQ     

        public Guid TransferSettingsId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
