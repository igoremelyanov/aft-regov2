using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;

namespace AFT.RegoV2.Core.Brand.Interface.ApplicationServices
{
    public interface IBrandQueries
    {
        IQueryable<Licensee> GetAllLicensees();

        IQueryable<Licensee> GetLicensees();

        Licensee GetLicensee(Guid licenseeId);

        IEnumerable<Contract> GetLicenseeContracts(Guid licenseeId);

        bool CanActivateLicensee(Licensee licensee);

        bool CanRenewLicenseeContract(Licensee licensee);
        IQueryable<Licensee> GetFilteredLicensees();
        IQueryable<Data.Brand> GetAllBrands();

        IQueryable<Data.Brand> GetBrands();
        Data.Brand GetBrandOrNull(Guid brandId);

        PlayerActivationMethod GetPlayerActivationMethod(Guid brandId);

        Task<Data.Brand> GetBrandOrNullAsync(Guid brandId);

        Data.Brand GetBrand(Guid brandId);

        IEnumerable<Data.Brand> GetBrandsByLicensee(Guid licenseeId);

        bool IsBrandActive(Guid brandId);

        bool DoesBrandExist(Guid brandId);

        bool BrandHasCurrency(Guid brandId, string currencyCode);

        bool BrandHasCountry(Guid brandId, string countryCode);

        bool BrandHasCulture(Guid brandId, string cultureCode);

        bool IsCountryAssignedToAnyBrand(string countryCode);

        IQueryable<Data.Brand> GetFilteredBrands(IEnumerable<Interface.Data.Brand> brands, Guid userId);
        IQueryable<Culture> GetCultures();

        Culture GetCulture(string cultureCode);
        IEnumerable<BrandCulture> GetAllBrandCultures();
        IEnumerable<Culture> GetCulturesByBrand(Guid brandId);
        IEnumerable<Culture> GetAllowedCulturesByBrand(Guid brandId);
        IEnumerable<Culture> GetActiveCultures();
        IQueryable<Country> GetAllCountries();

        IQueryable<Country> GetCountries();
        Country GetCountry(string countryCode);
        IEnumerable<BrandCountry> GetAllBrandCountries();
        IEnumerable<Country> GetCountriesByBrand(Guid brandId);

        IEnumerable<Country> GetAllowedCountriesByBrand(Guid brandId);
        IQueryable<Currency> GetCurrencies();

        IEnumerable<Currency> GetCurrenciesByBrand(Guid brandId);
        Guid? GetDefaultPaymentLevelId(Guid brandId, string currencyCode);
        IEnumerable<string> GetFilteredCurrencies(Guid brandId);
        IQueryable<VipLevel> GetVipLevels();

        IQueryable<VipLevel> GetAllVipLevels();
        VipLevel GetVipLevel(Guid vipLevelId);
        IQueryable<VipLevel> GetFilteredVipLevels(IQueryable<VipLevel> vipLevels, Guid userId);
        VipLevelViewModel GetVipLevelViewModel(Guid vipLevelId);
        IEnumerable<LicenseeProduct> GetAllowedProductsByBrand(Guid brandId);
        IEnumerable<BrandProduct> GetAllowedProducts(Guid userId, Guid? brandId = null);

        WalletTemplate GetWalletTemplate(Guid id);

        IEnumerable<WalletTemplate> GetWalletTemplates(Guid brandId);

        IQueryable<WalletListDTO> GetWalletTemplates();
        IEnumerable<RiskLevel> GetRiskLevels(Guid brandId);
    }
}
