using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class BrandCurrenciesAssigned : DomainEventBase
    {
        public Guid BrandId { get; set; }
        public string[] Currencies { get; set; }
        public string DefaultCurrency { get; set; }
        public string BaseCurrency { get; set; }
    }
}