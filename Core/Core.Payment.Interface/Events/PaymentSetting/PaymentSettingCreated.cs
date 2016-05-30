using System;
using AFT.RegoV2.Core.Common.Interfaces;


namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentSettingCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string VipLevel { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }

        public PaymentSettingCreated()
        {
        }
    }
}
