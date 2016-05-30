using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Auth;
using AFT.RegoV2.AdminApi.Interface.Brand;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using BrandId = AFT.RegoV2.AdminApi.Interface.Brand.BrandId;
using Country = AFT.RegoV2.AdminApi.Interface.Brand.Country;
using Currency = AFT.RegoV2.AdminApi.Interface.Brand.Currency;
using Licensee = AFT.RegoV2.AdminApi.Interface.Brand.Licensee;
using VipLevel = AFT.RegoV2.AdminApi.Interface.Brand.VipLevel;

namespace AFT.RegoV2.AdminApi.Controllers.Brand
{
    [Authorize]
    public class BrandController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly IAdminQueries _adminQueries;

        public BrandController(
            BrandQueries brandQueries,
            BrandCommands brandCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _brandQueries = brandQueries;
            _brandCommands = brandCommands;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetUserBrands)]
        public UserBrandsResponse GetUserBrands()
        {
            var brands = _brandQueries.GetBrands().ToList();
            var filteredBrands = _brandQueries.GetFilteredBrands(brands, UserId);
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            return new UserBrandsResponse
            {
                Brands = filteredBrands
                    .OrderBy(x => x.Licensee.Name)
                    .ThenBy(x => x.Name)
                    .Select(b => new UserBrand
                    {
                        Id = b.Id,
                        Name = b.Name,
                        LicenseeId = b.Licensee.Id,
                        Currencies = b.BrandCurrencies.Select(c => new Currency
                        {
                            Code = c.Currency.Code,
                            Name = c.Currency.Name
                        }),
                        VipLevels = b.VipLevels.Select(v => new VipLevel
                        {
                            Code = v.Code,
                            Name = v.Name
                        }),
                        IsSelectedInFilter = brandFilterSelections.Contains(b.Id)
                    }).ToList()
            };
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListBrands)]
        public SearchPackageResult GetBrands([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.BrandManager);

            var brands = _brandQueries.GetFilteredBrands(_brandQueries.GetAllBrands(), UserId);

            var dataBuilder = new SearchPackageDataBuilder<Core.Brand.Interface.Data.Brand>(searchPackage, brands);

            dataBuilder.Map(brand => brand.Id, brand => new[]
            {
                brand.Code,
                brand.Name,
                brand.Licensee.Name,
                brand.Type.ToString(),
                brand.Status.ToString(),
                brand.PlayerPrefix,
                Enum.GetName(typeof (PlayerActivationMethod), brand.PlayerActivationMethod),
                brand.DefaultCulture ?? string.Empty,
                brand.CreatedBy,
                Format.FormatDate(brand.DateCreated, false),
                brand.Remarks,
                brand.UpdatedBy,
                Format.FormatDate(brand.DateUpdated, false),
                brand.ActivatedBy,
                Format.FormatDate(brand.DateActivated, false),
                brand.DeactivatedBy,
                Format.FormatDate(brand.DateDeactivated, false),
            });

            return dataBuilder.GetPageData(brand => brand.Name);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandAddData)]
        public BrandAddDataResponse GetAddData()
        {
            VerifyPermission(Permissions.Create, Modules.BrandManager);

            var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();

            var licensees = _brandQueries.GetFilteredLicensees()
                .Where(x => licenseeFilterSelections.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name)
                .ToArray();

            var brands = _brandQueries.GetFilteredBrands(_brandQueries.GetBrands(), UserId).ToArray();

            return new BrandAddDataResponse
            {
                Licensees = licensees.Select(licensee => new Licensee
                {
                    Id = licensee.Id,
                    Name = licensee.Name,
                    PrefixRequired = brands.Any(brand =>
                        brand.Licensee.Id == licensee.Id &&
                        brand.PlayerPrefix != null)
                }),
                Types = Enum.GetNames(typeof (BrandType)).OrderBy(x => x),
                TimeZones = TimeZoneInfo.GetSystemTimeZones().Select(x => new Timezone { Id = x.Id, DisplayName = x.DisplayName}),
                PlayerActivationMethods = Enum.GetNames(typeof (PlayerActivationMethod)).OrderBy(x => x),
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandEditData)]
        public BrandEditDataResponse GetEditData(Guid id)
        {
            VerifyPermission(Permissions.Update, Modules.BrandManager);

            CheckBrand(id);

            var licensees = _brandQueries.GetFilteredLicensees()
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name)
                .ToArray();

            var brands = _brandQueries.GetBrands().ToArray();

            var brand = brands.FirstOrDefault(x => x.Id == id);

            var brandData = brand == null
                ? null
                : new BrandDataResponseBase
                {
                    Licensee = brand.Licensee.Id,
                    Type = Enum.GetName(typeof(BrandType), brand.Type),
                    Name = brand.Name,
                    Code = brand.Code,
                    Email = brand.Email,
                    SmsNumber = brand.SmsNumber,
                    WebsiteUrl = brand.WebsiteUrl,
                    EnablePlayerPrefix = brand.EnablePlayerPrefix,
                    PlayerPrefix = brand.PlayerPrefix,
                    PlayerActivationMethod = Enum.GetName(typeof(PlayerActivationMethod), brand.PlayerActivationMethod),
                    InternalAccounts = brand.InternalAccountsNumber,
                    TimeZoneId = brand.TimezoneId,
                    Remarks = brand.Remarks
                };

            return new BrandEditDataResponse
            {
                Licensees = licensees.Select(x => new Licensee
                {
                    Id = x.Id,
                    Name = x.Name,
                    PrefixRequired = brands.Any(y =>
                            y.Licensee.Id == x.Id &&
                            y.PlayerPrefix != null)
                }),
                Types = Enum.GetNames(typeof(BrandType)).OrderBy(x => x),
                TimeZones = TimeZoneInfo.GetSystemTimeZones().Select(x => new Timezone { Id = x.Id, DisplayName = x.DisplayName }),
                PlayerActivationMethods = Enum.GetNames(typeof(PlayerActivationMethod)).OrderBy(x => x),
                Brand = brandData
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandViewData)]
        public BrandViewDataResponse GetViewData(Guid id)
        {
            VerifyPermission(Permissions.View, Modules.BrandManager);

            CheckBrand(id);

            var brand = _brandQueries.GetBrandOrNull(id);

            return new BrandViewDataResponse
            {
                Licensee = brand.Licensee.Name,
                Type = Enum.GetName(typeof(BrandType), brand.Type),
                Name = brand.Name,
                Code = brand.Code,
                Email = brand.Email,
                SmsNumber = brand.SmsNumber,
                WebsiteUrl = brand.WebsiteUrl,
                EnablePlayerPrefix = brand.EnablePlayerPrefix,
                PlayerPrefix = brand.PlayerPrefix,
                PlayerActivationMethod = Enum.GetName(typeof(PlayerActivationMethod), brand.PlayerActivationMethod),
                InternalAccounts = brand.InternalAccountsNumber,
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(brand.TimezoneId).DisplayName,
                Remarks = brand.Remarks,
                Status = Enum.GetName(typeof(BrandStatus), brand.Status)
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.AddBrand)]
        public AddBrandResponse Add(AddBrandRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.BrandManager);

            var validationResult = _brandCommands.ValidateThatBrandCanBeAdded(request);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<AddBrandResponse>(validationResult);

            var id = _brandCommands.AddBrand(request);
            return new AddBrandResponse { Success = true, Data = new BrandId { Id = id } };
        }

        [HttpPost]
        [Route(AdminApiRoutes.EditBrand)]
        public EditBrandResponse Edit(EditBrandRequest request)
        {
            VerifyPermission(Permissions.Update, Modules.BrandManager);
            CheckBrand(request.Brand);

            var validationResult = _brandCommands.ValidateThatBrandCanBeEdited(request);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<EditBrandResponse>(validationResult);

            _brandCommands.EditBrand(request);
            return new EditBrandResponse { Success = true };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandCountries)]
        public BrandCountriesResponse GetCountries(Guid brandId)
        {
            VerifyAnyPermission(
                new Permission(Permissions.View, Modules.BrandManager),
                new Permission(Permissions.Update, Modules.PlayerManager));

            CheckBrand(brandId);

            return new BrandCountriesResponse
            {
                Countries =
                    _brandQueries.GetCountriesByBrand(brandId).Select(x => new Country {Code = x.Code, Name = x.Name})
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivateBrand)]
        public ActivateBrandResponse Activate(ActivateBrandRequest request)
        {
            VerifyPermission(Permissions.Activate, Modules.BrandManager);
            CheckBrand(request.BrandId);

            var validationResult = _brandCommands.ValidateThatBrandCanBeActivated(request);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<ActivateBrandResponse>(validationResult);

            _brandCommands.ActivateBrand(request);
            return new ActivateBrandResponse { Success = true };
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivateBrand)]
        public DeactivateBrandResponse Deactivate(DeactivateBrandRequest request)
        {
            VerifyPermission(Permissions.Deactivate, Modules.BrandManager);
            CheckBrand(request.BrandId);

            var validationResult = _brandCommands.ValidateThatBrandCanBeDeactivated(request);

            if (!validationResult.IsValid)
                return ValidationErrorResponse<DeactivateBrandResponse>(validationResult);

            _brandCommands.DeactivateBrand(request);

            return new DeactivateBrandResponse { Success = true };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrands)]
        public BrandsResponse Brands([FromUri] bool useFilter, [FromUri] Guid[] licensees)
        {
            var brands = _brandQueries.GetBrands();

            if (licensees != null)
                brands = brands.Where(x => licensees.Contains(x.Licensee.Id));

            brands = _brandQueries.GetFilteredBrands(brands, UserId);

            if (useFilter)
            {
                var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

                brands = brands.Where(x => brandFilterSelections.Contains(x.Id));
            }

            return new BrandsResponse
            {
                Brands = brands.OrderBy(x => x.Name).Select(x => new Interface.Brand.Brand { Id = x.Id, Name = x.Name })
            };
        }
    }
}