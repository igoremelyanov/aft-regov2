using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class PlayerPaymentAmount
    {
        public Guid PlayerId { get; set; }
        public string LicenseeName { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PaymentLevelName { get; set; }
        public Guid PaymentLevelId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}