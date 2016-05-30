using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class TransferFundSettingsCreated : DomainEventBase
    {
        public Guid TransferSettingsId { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
