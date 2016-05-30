using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class CurrencyExchangeCreated : DomainEventBase
    {
        public Guid BrandId { get; set; }
        public string CurrencyToCode { get; set; }
        public decimal CurrentRate { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}
