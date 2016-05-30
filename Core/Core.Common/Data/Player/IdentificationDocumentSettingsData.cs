using System;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class IdentificationDocumentSettingsData
    {
        public Guid Id { get; set; }

        public Guid LicenseeId { get; set; }

        public Guid BrandId { get; set; }

        public TransactionType? TransactionType { get; set; }

        public Guid PaymentGatewayBankAccountId { get; set; }

        public PaymentMethod PaymentGatewayMethod { get; set; }

        public bool IdFront { get; set; }

        public bool IdBack { get; set; }

        public bool CreditCardFront { get; set; }

        public bool CreditCardBack { get; set; }

        public bool POA { get; set; }

        public bool DCF { get; set; }

        public string Remark { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
    }

    public enum TransactionType
    {
        Withdraw,
        Deposit
    }
}
