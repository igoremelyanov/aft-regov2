using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class TransferFundSettingsActivated : DomainEventBase
    {
        public Guid TransferSettingsId { get; set; }
        public string EnabledBy { get; set; }
        public DateTimeOffset? Activated { get; set; }
        public string Remarks { get; set; }
    }
}
