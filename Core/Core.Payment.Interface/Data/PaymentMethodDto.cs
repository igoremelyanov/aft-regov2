using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class PaymentMethodDto
    {
        public const string OfflinePayMethod = "Offline-Bank";
        public string Id { get; set; }
        public string Name { get; set; }
        public PaymentMethod PaymentGatewayMethod { get; set; }
    }
}