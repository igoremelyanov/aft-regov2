using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class SavePaymentSettingsCommand
    {
        public Guid Id { get; set; }
        public Guid Licensee { get; set; }
        public Guid Brand { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentMethod PaymentGatewayMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string Currency { get; set; }
        public string VipLevel { get; set; }
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }
        public int MaxTransactionPerDay { get; set; }
        public int MaxTransactionPerWeek { get; set; }
        public int MaxTransactionPerMonth { get; set; }
    }

    public class PaymentSettingSaveResult
    {
        public string Message { get; set; }
        public Guid PaymentSettingsId { get; set; }
    }

    public class PaymentSettingsId
    {
        private readonly Guid _id;

        public PaymentSettingsId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(PaymentSettingsId id)
        {
            return id._id;
        }

        public static implicit operator PaymentSettingsId(Guid id)
        {
            return new PaymentSettingsId(id);
        }
    }
}