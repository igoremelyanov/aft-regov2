using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminApi.Interface.Brand
{
    public class UserBrandsResponse
    {
        public IEnumerable<UserBrand> Brands { get; set; }
    }

    public class UserBrand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LicenseeId { get; set; }
        public IEnumerable<Currency> Currencies { get; set; }
        public IEnumerable<VipLevel> VipLevels { get; set; }
        public bool IsSelectedInFilter { get; set; }
    }

    public class Currency
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class VipLevel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}