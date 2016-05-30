using System;
using AFT.RegoV2.Core.Common.Data.Base;

namespace AFT.RegoV2.Core.Brand.Interface.Data
{
    public class DeactivateBrandRequest : ValidationResponseBase
    {
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }
    }
}
