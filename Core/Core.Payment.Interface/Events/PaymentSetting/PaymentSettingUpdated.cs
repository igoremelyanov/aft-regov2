using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentSettingUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string VipLevel { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }
    }
}
