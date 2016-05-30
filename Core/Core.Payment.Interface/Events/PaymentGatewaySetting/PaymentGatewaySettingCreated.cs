using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentGatewaySettingCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public Guid BrandId { get; set; }
        public string OnlinePaymentMethodName { get; set; }
        public string PaymentGatewayName { get; set; }
        public int Channel { get; set; }
        public string EntryPoint { get; set; }
    }
}
