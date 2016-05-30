using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class TransferFundSettingsDeactivated : DomainEventBase
    {
        public TransferFundSettingsDeactivated() { } // default constructor is required for publishing event to MQ

        public Guid TransferSettingsId { get; set; }
        public string DisabledBy { get; set; }
        public DateTimeOffset? Deactivated { get; set; }
        public string Remarks { get; set; }
    }
}
