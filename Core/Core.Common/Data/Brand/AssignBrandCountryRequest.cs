using System;

namespace AFT.RegoV2.Core.Common.Data.Brand
{
    public class AssignBrandCountryRequest
    {
        public Guid Brand { get; set; }
        public string[] Countries { get; set; }
    }
}