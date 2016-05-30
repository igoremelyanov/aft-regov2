using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentGatewaySettingDeactivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset DeactivatedDate { get; set; }

        public Guid BrandId { get; set; }
        public string OnlinePaymentMethodName { get; set; }
        public string PaymentGatewayName { get; set; }
        public int Channel { get; set; }
        public string EntryPoint { get; set; }
        public string Remarks { get; set; }
    }
}
