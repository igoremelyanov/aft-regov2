using System;
using System.Collections.Generic;
using AFT.RegoV2.Shared.ApiDataFiltering;

namespace AFT.RegoV2.Bonus.Api.Interface.Requests
{
    public class BrandFilteredDataRequest
    {
        public FilteredDataRequest DataRequest { get; set; }
        public List<Guid> BrandFilters { get; set; }
    }
}