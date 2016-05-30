using System;
using AFT.RegoV2.Shared.ApiDataFiltering;

namespace AFT.RegoV2.Bonus.Api.Interface.Requests
{
    public class RedemptionFilteredDataRequest
    {
        public FilteredDataRequest DataRequest { get; set; }
        public Guid BonusRedemptionId { get; set; }
    }
}