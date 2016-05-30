using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Brand.ApplicationServices 
{
    public class BrandQueries : MarshalByRefObject, IApplicationService, IBrandQueries
    {
        private readonly IBrandRepository _repository;
        private readonly ISecurityRepository _securityRepository;
        private readonly IActorInfoProvider _actorInfoProvider;

        public BrandQueries(
            IBrandRepository repository,
            ISecurityRepository securityRepository,
            IActorInfoProvider actorInfoProvider)
        {
            _repository = repository;
            _securityRepository = securityRepository;
            _actorInfoProvider = actorInfoProvider;
        }

        #region licensee
        [Filtered]
        [Permission(Permissions.View, Module = Modules.LicenseeManager)]
        public IQueryable<Licensee> GetAllLicensees()
        {
            return GetLicensees();
        }

        public IQueryable<Licensee> GetLicensees()
        {
            return _repository.Licensees
                .Include(x => x.Brands)
                .Include(x => x.Currencies)
                .Include(x => x.Cultures)
                .Include(x => x.Countries)
                .Include(x => x.Products)
                .Include(x => x.Contracts)
                .Include(l => l.Brands.Select(b => b.BrandCurrencies));
        }

        public Licensee GetLicensee(Guid licenseeId)
        {
            return GetLicensees().SingleOrDefault(x => x.Id == licenseeId);
        }

        public IEnumerable<Contract> GetLicenseeContracts(Guid licenseeId)
        {
            var licensee = GetLicensee(licenseeId);

            return licensee == null ? null : licensee.Contracts;
        }

        public bool CanActivateLicensee(Licensee licensee)
        {
            return (licensee.Status == LicenseeStatus.Inactive || licensee.Status == LicenseeStatus.Deactivated) &&
                   (!licensee.ContractEnd.HasValue || licensee.ContractEnd > DateTimeOffset.UtcNow);
        }

        public bool CanRenewLicenseeContract(Licensee licensee)
        {
            return licensee.ContractEnd.HasValue &&
                licensee.ContractEnd.Value < DateTimeOffset.UtcNow &&
                licensee.Status == LicenseeStatus.Active;
        }

        /// <summary>
        /// Gets current admin filtered licensees
        /// </summary>
        public IQueryable<Licensee> GetFilteredLicensees()
        {
            var licensees = GetLicensees();

            var admin = _securityRepository.Admins
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Single(u => u.Id == _actorInfoProvider.Actor.Id);

            var adminLicenseeIds = admin.Licensees.Select(l => l.Id);

            return (admin.Role.Id == RoleIds.SuperAdminId
                ? licensees
                : licensees.Where(l =>
                    adminLicenseeIds.Any(x => l.Id == x) 
                    && l.Status == LicenseeStatus.Active)
                    ).AsQueryable();
        }
        #endregion licensee

        #region brand
        [Filtered]
        public IQueryable<Interface.Data.Brand> GetAllBrands()
        {
            return GetBrands();
        }

        public IQueryable<Interface.Data.Brand> GetBrands()
        {
            return _repository.Brands
                .Include(x => x.BrandCountries.Select(y => y.Country))
                .Include(x => x.BrandCountries.Select(y => y.Brand))
                .Include(x => x.BrandCultures.Select(y => y.Culture))
                .Include(x => x.BrandCurrencies.Select(y => y.Currency))
                .Include(x => x.Licensee)
                .Include(x => x.Licensee.Brands)
                .Include(x => x.Licensee.Currencies)
                .Include(x => x.Licensee.Cultures)
                .Include(x => x.Licensee.Countries)
                .Include(x => x.Licensee.Products)
                .Include(x => x.Licensee.Contracts)
                .Include(x => x.BrandCurrencies)
                .Include(x => x.VipLevels)
                .Include(x => x.WalletTemplates)
                .Include(x => x.Products.Select(y => y.Brand));
        }

        public Interface.Data.Brand GetBrandOrNull(Guid brandId)
        {
            return GetBrands().SingleOrDefault(x => x.Id == brandId);
        }

        public PlayerActivationMethod GetPlayerActivationMethod(Guid brandId)
        {
            return _repository.Brands.Find(brandId).PlayerActivationMethod;
        }

        public async Task<Interface.Data.Brand> GetBrandOrNullAsync(Guid brandId)
        {
            return await GetBrands().SingleOrDefaultAsync(x => x.Id == brandId);
        }

        public Interface.Data.Brand GetBrand(Guid brandId)
        {
            var result = GetBrandOrNull(brandId);
            if (result == null)
                throw new RegoException(string.Format("Unable to find brand with Id '{0}'", brandId));

            return result;
        }

        public Interface.Data.Brand GetBrand(string brandCode)
        {
            var result = _repository.Brands.SingleOrDefault(x => x.Code == brandCode);

            if (result == null)
                throw new RegoException(string.Format("Unable to find brand with Code '{0}'", brandCode));

            return result;
        }

        public IEnumerable<Interface.Data.Brand> GetBrandsByLicensee(Guid licenseeId)
        {
            var licensee = GetLicensee(licenseeId);

            return licensee == null ? null : licensee.Brands;
        }

        public bool IsBrandActive(Guid brandId)
        {
            return _repository.Brands.Any(x => x.Id == brandId && x.Status == BrandStatus.Active);
        }

        public bool DoesBrandExist(Guid brandId)
        {
            return _repository.Brands.Find(brandId) != null;
        }

        public bool BrandHasCurrency(Guid brandId, string currencyCode)
        {
            return _repository.Brands
                .Include(b => b.BrandCurrencies.Select(x => x.Currency))
                .Any(x => x.Id == brandId && x.BrandCurrencies.Any(y => y.CurrencyCode == currencyCode));
        }

        public bool BrandHasCountry(Guid brandId, string countryCode)
        {
            return _repository.Brands
                .Include(b => b.BrandCountries.Select(x => x.Country))
                .Any(x => x.Id == brandId && x.BrandCountries.Any(y => y.CountryCode == countryCode));
        }

        public bool BrandHasCulture(Guid brandId, string cultureCode)
        {
            return _repository.Brands
                .Include(b => b.BrandCultures.Select(x => x.Culture))
                .Any(x => x.Id == brandId && x.BrandCultures.Any(y => y.CultureCode == cultureCode));
        }

        public bool IsCountryAssignedToAnyBrand(string countryCode)
        {
            return GetBrands().Any(x => x.BrandCountries.Any(y => y.CountryCode == countryCode));
        }

        public IQueryable<Interface.Data.Brand> GetFilteredBrands(IEnumerable<Interface.Data.Brand> brands, Guid userId)
        {
            var user = _securityRepository.Admins
                .Include(u => u.AllowedBrands)
                .Include(u => u.Licensees)
                .Include(u => u.Role)
                .Single(u => u.Id == userId);

            var brandQuery = brands.AsQueryable();

            var allowedBrandIds = user.AllowedBrands.Select(x => x.Id).ToArray();

            if (user.Role.Id == RoleIds.MultipleBrandManagerId || user.Role.Id == RoleIds.SingleBrandManagerId)
            {
                return brandQuery.Where(x => allowedBrandIds.Contains(x.Id));
            }

            if (user.Role.Id == RoleIds.LicenseeId)
            {
                var licenseeId = user.Licensees.First().Id;
                
                var licensee = _repository.Licensees
                    .Include(x => x.Brands)
                    .First(x => x.Id == licenseeId);

                var allowedBrandIdsForLicensee = licensee.Brands
                    .Where(x => allowedBrandIds.Contains(x.Id))
                    .Select(x => x.Id);

                return brandQuery.Where(x => allowedBrandIdsForLicensee.Contains(x.Id));
            }

            return allowedBrandIds.Any()
                ? brandQuery.Where(x => allowedBrandIds.Contains(x.Id))
                : brandQuery;
        }

        #endregion brand

        #region culture

        public IQueryable<Culture> GetCultures()
        {
            return _repository.Cultures.AsNoTracking();
        }

        public Culture GetCulture(string cultureCode)
        {
            return GetCultures().SingleOrDefault(c => c.Code == cultureCode);
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.SupportedLanguages)]
        public IEnumerable<BrandCulture> GetAllBrandCultures()
        {
            var brands = GetBrands().ToDictionary(b => b.Id, b => b);
            var brandCultures = GetBrands().SelectMany(b => b.BrandCultures);

            brandCultures.ForEach(bc =>
            {
                bc.Brand = brands[bc.BrandId];
            });

            return brandCultures;
        }

        public IEnumerable<Culture> GetCulturesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.BrandCultures.Select(x => x.Culture);
        }

        public IEnumerable<Culture> GetAllowedCulturesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.Licensee.Cultures;
        }

        public IEnumerable<Culture> GetActiveCultures()
        {
            return GetCultures().Where(x => x.Status == CultureStatus.Active);
        }

        #endregion culture

        #region country

        [Permission(Permissions.View, Module = Modules.CountryManager)]
        public IQueryable<Country> GetAllCountries()
        {
            return GetCountries();
        }

        public IQueryable<Country> GetCountries()
        {
            return _repository.Countries.AsNoTracking();
        }

        public Country GetCountry(string countryCode)
        {
            return GetCountries().SingleOrDefault(x => x.Code == countryCode);
        }

        [Filtered]
        public IEnumerable<BrandCountry> GetAllBrandCountries()
        {
            var countries = GetCountries().ToDictionary(c => c.Code, c => c);
            var brands = GetBrands().ToDictionary(b => b.Id, b => b);
            var brandCountries = GetBrands().SelectMany(b => b.BrandCountries);
            brandCountries.ForEach(bc =>
            {
                bc.Brand = brands[bc.BrandId];
                bc.Country = countries[bc.CountryCode];
            });

            return brandCountries;
        }

        public IEnumerable<Country> GetCountriesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.BrandCountries.Select(x => x.Country);
        }

        public IEnumerable<Country> GetAllowedCountriesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.Licensee.Countries;
        }

        #endregion country

        #region currency

        public IQueryable<Currency> GetCurrencies()
        {
            return _repository.Currencies.AsNoTracking();
        }

        public IEnumerable<Currency> GetCurrenciesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.BrandCurrencies.Select(x => x.Currency);
        }

        public Guid? GetDefaultPaymentLevelId(Guid brandId, string currencyCode)
        {
            var brand = _repository
                .Brands
                .Include(o => o.BrandCurrencies)
                .Single(x => x.Id == brandId);

            var brandCurrency = brand.BrandCurrencies
                .Single(o => o.CurrencyCode == currencyCode);

            var defaultPaymentLevelId = brandCurrency.DefaultPaymentLevelId;

            return defaultPaymentLevelId;
        }

        public IEnumerable<string> GetFilteredCurrencies(Guid brandId)
        {
            var brandCurrencies = GetCurrenciesByBrand(brandId).Select(x => x.Code);
            var admin = _securityRepository.Admins
                .Include(x => x.Role)
                .Include(u => u.Currencies)
                .Single(u => u.Id == _actorInfoProvider.Actor.Id);

            var adminCurrencies = admin.Currencies.Select(l => l.Currency);

            return (admin.Role.Id == RoleIds.SuperAdminId)
                ? brandCurrencies
                : brandCurrencies.Where(adminCurrencies.Contains);
        }
        #endregion currency

        #region vip

        [Permission(Permissions.View, Module = Modules.VipLevelManager)]
        public IQueryable<VipLevel> GetVipLevels()
        {
            return _repository.VipLevels
                .Include(x => x.Brand.Licensee)
                .Include(x => x.VipLevelGameProviderBetLimits.Select(y => y.Currency))
                .AsNoTracking();
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.VipLevelManager)]
        public IQueryable<VipLevel> GetAllVipLevels()
        {
            return GetVipLevels();
        }

        public VipLevel GetVipLevel(Guid vipLevelId)
        {
            return GetVipLevels().SingleOrDefault(x => x.Id == vipLevelId);
        }

        public IQueryable<VipLevel> GetFilteredVipLevels(IQueryable<VipLevel> vipLevels, Guid userId)
        {
            var user = _securityRepository
                .Admins
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Include(x => x.AllowedBrands)
                .Single(x => x.Id == userId);

            var userRoleId = user.Role.Id;

            if (userRoleId == RoleIds.SuperAdminId)
                return vipLevels;

            if (userRoleId == RoleIds.SingleBrandManagerId || userRoleId == RoleIds.MultipleBrandManagerId)
            {
                var allowedBrandIds = user.AllowedBrands.Select(x => x.Id);

                return vipLevels.Where(x => allowedBrandIds.Contains(x.Brand.Id));
            }

            if (userRoleId == RoleIds.LicenseeId)
            {
                var licensee = GetLicensee(user.Licensees.First().Id);

                if (licensee == null)
                    return null;

                var allowedLicenseeBrandsIds = licensee.Brands.Select(x => x.Id);

                return vipLevels.Where(x => allowedLicenseeBrandsIds.Contains(x.Brand.Id));
            }

            return null;
        }

        public VipLevelViewModel GetVipLevelViewModel(Guid vipLevelId)
        {
            var entity = GetVipLevel(vipLevelId);
            if (entity == null)
                return null;

            var vipLevel = new VipLevelViewModel
            {
                Id = entity.Id,
                Brand = entity.Brand.Id,
                Code = entity.Code,
                Name = entity.Name,
                Rank = entity.Rank,
                Description = entity.Description,
                Color = entity.ColorCode,
                Limits = entity
                    .VipLevelGameProviderBetLimits
                    .Select(x => new VipLevelLimitViewModel
                    {
                        Id = x.Id,
                        CurrencyCode = x.Currency.Code,
                        GameProviderId = x.GameProviderId,
                        BetLimitId = x.BetLimitId
                    }).ToList(),
            };

            return vipLevel;
        }

        #endregion vip

        #region products

        public IEnumerable<LicenseeProduct> GetAllowedProductsByBrand(Guid brandId)
        {
            return GetBrandOrNull(brandId).Licensee.Products;
        }

        [Permission(Permissions.View, Module = Modules.SupportedProducts)]
        public IEnumerable<BrandProduct> GetAllowedProducts(Guid userId, Guid? brandId = null)
        {
            var queryable = GetBrands();

            if (brandId.HasValue)
            {
                queryable = queryable.Where(x => x.Id == brandId);
            }

            var allowedBrands = GetFilteredBrands(queryable, userId);

            return allowedBrands.SelectMany(x => x.Products).Include(x => x.Brand);
        }

        #endregion products

        #region wallet

        public WalletTemplate GetWalletTemplate(Guid id)
        {
            var walletTemplate = _repository
                .WalletTemplates
                .Include(x => x.Brand)
                .FirstOrDefault(x => x.Id == id);
            return walletTemplate;
        }

        public IEnumerable<WalletTemplate> GetWalletTemplates(Guid brandId)
        {
            var walletTemplates = _repository
                .Brands
                .Include(x => x.WalletTemplates.Select(wt => wt.WalletTemplateProducts))
                .Single(x => x.Id == brandId)
                .WalletTemplates;

            return walletTemplates;
        }

        [Permission(Permissions.View, Module = Modules.WalletManager)]
        public IQueryable<WalletListDTO> GetWalletTemplates()
        {
            var walletTemplates = _repository
                .WalletTemplates
                .Include(x => x.Brand)
                .Include(x => x.Brand.Licensee)
                .ToList();

            var grouped = walletTemplates
                .GroupBy(x => x.Brand.Id)
                .SelectMany(x => x.OrderBy(w => w.DateUpdated))
                .ToList();

            var listOfDtos = new List<WalletListDTO>();

            foreach (var walletTemplate in grouped)
            {
                if (!walletTemplate.IsMain)
                    continue;

                listOfDtos.Add(new WalletListDTO
                {
                    Brand = walletTemplate.Brand.Id,
                    BrandName = walletTemplate.Brand.Name,
                    LicenseeId = walletTemplate.Brand.Licensee.Id,
                    LicenseeName = walletTemplate.Brand.Licensee.Name,
                    CreatedBy = walletTemplate.CreatedBy == Guid.Empty ? "" : _securityRepository.Admins.First(u => u.Id == walletTemplate.CreatedBy).FirstName,
                    DateCreated = walletTemplate.DateCreated,
                    DateUpdated = walletTemplate.DateUpdated,
                    UpdatedBy = walletTemplate.UpdatedBy == Guid.Empty ? "" : _securityRepository.Admins.First(u => u.Id == walletTemplate.UpdatedBy).FirstName
                });
            }

            return listOfDtos.AsQueryable();
        }

        #endregion wallet

        #region fraud

        public IEnumerable<RiskLevel> GetRiskLevels(Guid brandId)
        {
            return _repository.RiskLevels.Where(x => x.BrandId == brandId);
        }

        #endregion fraud
    }
}