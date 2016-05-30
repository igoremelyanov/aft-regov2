using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentSettingDeactivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset DeactivatedDate { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }

        public PaymentSettingDeactivated()
        {
        }
    }
}
