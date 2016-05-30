using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentLevelEdited : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string CurrencyCode { get; set; }
        public PaymentLevelStatus Status { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }

        public PaymentLevelEdited(){}

        public PaymentLevelEdited(
            Guid id,
            string code,
            string name,
            Guid brandId,
            string currencyCode,
            Data.PaymentLevelStatus status,
            string updatedBy,
            DateTimeOffset updatedDate)
        {
            Id = id;
            Code = code;
            Name = name;
            BrandId = brandId;
            CurrencyCode = currencyCode;
            Status = status;
            UpdatedBy = updatedBy;
            UpdatedDate = updatedDate;
        }
    }
}
