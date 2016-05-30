using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentLevelAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string CurrencyCode { get; set; }
        public Data.PaymentLevelStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public PaymentLevelAdded() {}
        public PaymentLevelAdded(
            Guid id,
            string code,
            string name,
            Guid brandId,
            string currencyCode,
            Data.PaymentLevelStatus status,
            string createdBy,
            DateTimeOffset createdDate)
        {
            Id = id;
            Code = code;
            Name = name;
            BrandId = brandId;
            CurrencyCode = currencyCode;
            Status = status;
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }
    }
}
