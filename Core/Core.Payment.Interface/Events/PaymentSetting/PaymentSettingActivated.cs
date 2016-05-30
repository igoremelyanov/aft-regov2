using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentSettingActivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset ActivatedDate { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }
    }
}
