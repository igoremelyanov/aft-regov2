using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminApi.Interface.Brand
{
    public class BrandAddEditDataResponseBase
    {
        public IEnumerable<Licensee> Licensees { get; set; }
        public IEnumerable<string> Types { get; set; }
        public IEnumerable<Timezone> TimeZones { get; set; }
        public IEnumerable<string> PlayerActivationMethods { get; set; }
    }

    public class BrandDataResponseBase
    {
        public Guid Licensee { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public string SmsNumber { get; set; }
        public string WebsiteUrl { get; set; }
        public bool EnablePlayerPrefix { get; set; }
        public string PlayerPrefix { get; set; }
        public string PlayerActivationMethod { get; set; }
        public int InternalAccounts { get; set; }
        public string TimeZoneId { get; set; }
        public string Remarks { get; set; }
    }

    public class BrandAddDataResponse : BrandAddEditDataResponseBase
    {
    }

    public class BrandEditDataRequest
    {
        public Guid BrandId { get; set; }
    }

    public class BrandEditDataResponse : BrandAddEditDataResponseBase
    {
        public BrandDataResponseBase Brand { get; set; }
    }

    public class BrandViewDataResponse : BrandDataResponseBase
    {
        public new string Licensee { get; set; }
        public string TimeZone { get; set; }
        public string Status { get; set; }
    }

    public class BrandCountryAssignDataResponse
    {
        public IEnumerable<Country> AvailableCountries { get; set; }
        public IEnumerable<Country> AssignedCountries { get; set; }
        public bool IsActive { get; set; }
    }

    public class BrandCultureAssignDataResponse
    {
        public IEnumerable<Culture> AvailableCultures { get; set; }
        public IEnumerable<Culture> AssignedCultures { get; set; }
        public string DefaultCulture { get; set; }
        public bool IsActive { get; set; }
    }

    public class BrandCurrencyAssignDataResponse
    {
        public IEnumerable<Currency> AvailableCurrencies { get; set; }
        public IEnumerable<Currency> AssignedCurrencies { get; set; }
        public string DefaultCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetBrandCurrenciesResponse
    {
        public IEnumerable<string> CurrencyCodes { get; set; }
    }

    public class GetBrandCurrenciesWithNamesResponse
    {
        public IEnumerable<Currency> CurrencyCodes { get; set; }
    }

    public class Licensee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool PrefixRequired { get; set; }
    }

    public class Timezone
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}