using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Security.Helpers
{
    //todo: KB: this methods are suitable to Admin entity
    public static class AdminExtensions
    {
        public static void SetLicensees(this Admin admin, IEnumerable<Guid> licensees)
        {
            if (licensees == null) 
                return;

            admin.Licensees.Clear();

            licensees.Select(licensee => new LicenseeId { Id = licensee, AdminId = admin.Id })
                .ForEach(licenseeId =>
                {
                    admin.Licensees.Add(licenseeId);
                });
        }

        public static void SetAllowedBrands(this Admin admin, IEnumerable<Guid> allowedBrands)
        {
            if (allowedBrands == null) 
                return;

            admin.AllowedBrands.Clear();

            allowedBrands.Select(allowedBrandId => new BrandId { Id = allowedBrandId, AdminId = admin.Id })
                .ForEach(brandId => admin.AllowedBrands.Add(brandId));
        }

        public static void SetCurrencies(this Admin admin, IEnumerable<string> currencies)
        {
            if (currencies == null) 
                return;

            admin.Currencies.Clear();

            currencies.Select(currencyCode => new CurrencyCode { Currency = currencyCode, AdminId = admin.Id })
                .ForEach(currency =>
                {
                    admin.Currencies.Add(currency);
                });
        }

        public static void AddAllowedBrand(this Admin admin, Guid brandId)
        {
            if (admin.AllowedBrands.Any(b => b.Id == brandId)) 
                return;

            var allowedBrand = new BrandId { Id = brandId, AdminId = admin.Id };

            admin.AllowedBrands.Add(allowedBrand);
        }

        public static void RemoveAllowedBrand(this Admin admin, Guid brandId)
        {
            if (!admin.AllowedBrands.Any(b => b.Id == brandId && b.AdminId == admin.Id))
                return;

            var allowedBrand = admin.AllowedBrands.First(b => b.Id == brandId && b.AdminId == admin.Id);

            admin.AllowedBrands.Remove(allowedBrand);
        }
    }
}
