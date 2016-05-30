using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerPaymentLevelChanged : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public Guid OldPaymentLevelId { get; set; }
        public string OldPaymentLevelName { get; set; }
        public Guid NewPaymentLevelId { get; set; }
        public string NewPaymentLevelName { get; set; }
        public string Remarks { get; set; }
    }
}
