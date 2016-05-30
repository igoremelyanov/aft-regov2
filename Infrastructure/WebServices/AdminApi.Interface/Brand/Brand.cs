using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Base;

namespace AFT.RegoV2.AdminApi.Interface.Brand
{
    public class AddBrandResponse : ValidationResponseBase
    {
        public BrandId Data { get; set; }
    }

    public class EditBrandResponse : ValidationResponseBase
    {
    }

    public class ActivateBrandResponse : ValidationResponseBase
    {
    }

    public class DeactivateBrandResponse : ValidationResponseBase
    {
    }

    public class AssignBrandCountryResponse : ValidationResponseBase
    {
    }

    public class AssignBrandCultureResponse : ValidationResponseBase
    {
    }

    public class AssignBrandCurrencyResponse : ValidationResponseBase
    {
    }

    public class BrandCountriesResponse
    {
        public IEnumerable<Country> Countries { get; set; }
    }

    public class BrandsResponse
    {
        public IEnumerable<Brand> Brands { get; set; }
    }

    public class BrandId
    {
        public Guid Id { get; set; }
    }

    public class Brand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Country
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class Culture
    {
        public string Code { get; set; }
    }
}
