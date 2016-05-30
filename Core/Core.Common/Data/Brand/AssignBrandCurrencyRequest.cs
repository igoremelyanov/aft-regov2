using System;

namespace AFT.RegoV2.Core.Common.Data.Brand
{
    public class AssignBrandCurrencyRequest
    {
        public Guid Brand { get; set; }
        public string[] Currencies { get; set; }
        public string DefaultCurrency { get; set; }
        public string BaseCurrency { get; set; }
    }
}
