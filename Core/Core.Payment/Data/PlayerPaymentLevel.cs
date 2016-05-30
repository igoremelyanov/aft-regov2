using System;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class PlayerPaymentLevel
    {
        public Guid PlayerId { get; set; }
        public PaymentLevel PaymentLevel { get; set; }
    }
}