using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Brand;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Brand
{
    public class CurrencyViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string DefaultCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }

    [Authorize]
    public class BrandCurrencyController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly IAdminQueries _adminQueries;

        public BrandCurrencyController(
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
        [Route(AdminApiRoutes.ListBrandCurrencies)]
        [Filters.SearchPackageFilter("searchPackage")]
        public SearchPackageResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.SupportedCurrencies);
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandCurrencies)]
        public GetBrandCurrenciesResponse GetBrandCurrencies(Guid brandId)
        {
            VerifyPermission(Permissions.View, Modules.SupportedCurrencies);
            CheckBrand(brandId);

            var brand = _brandQueries.GetBrandOrNull(brandId);
            var currencyCodes = brand.BrandCurrencies.Select(c => c.CurrencyCode);

            return new GetBrandCurrenciesResponse
            {
                CurrencyCodes = currencyCodes
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandCurrenciesWithNames)]
        public GetBrandCurrenciesWithNamesResponse GetBrandCurrenciesWithNames(Guid brandId)
        {
            VerifyPermission(Permissions.View, Modules.SupportedCurrencies);
            CheckBrand(brandId);

            return new GetBrandCurrenciesWithNamesResponse
            {
                CurrencyCodes = _brandQueries.GetCurrenciesByBrand(brandId).Select(x => new Currency { Code = x.Code, Name = x.Name })
            };
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandCurrencyAssignData)]
        public BrandCurrencyAssignDataResponse GetAssignData(Guid brandId)
        {
            VerifyPermission(Permissions.Create, Modules.SupportedCurrencies);
            CheckBrand(brandId);

            var brand = _brandQueries.GetBrandOrNull(brandId);
            var licensee = _brandQueries.GetLicensees()
                .Single(l => l.Brands.Any(b => b.Id == brandId));
            var allowedCurrencies = licensee.Currencies.OrderBy(c => c.Name);
            var assignedCurrencies = brand.BrandCurrencies.Select(x => x.Currency).ToArray();
            var availableCurrencies = allowedCurrencies.Except(assignedCurrencies);

            return new BrandCurrencyAssignDataResponse
            {
                AvailableCurrencies = availableCurrencies.Select(x => new Currency {Code = x.Code}).ToList(),
                AssignedCurrencies = assignedCurrencies.Select(x => new Currency {Code = x.Code}).ToList(),
                DefaultCurrency = brand.DefaultCurrency,
                BaseCurrency = brand.BaseCurrency,
                IsActive = brand.Status == BrandStatus.Active
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.AssignBrandCurrency)]
        public AssignBrandCurrencyResponse Assign(AssignBrandCurrencyRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.SupportedCurrencies);
            CheckBrand(request.Brand);

            var validationResult = _brandCommands.ValidateThatBrandCurrencyCanBeAssigned(request);
            
            if (!validationResult.IsValid)
                return ValidationErrorResponse<AssignBrandCurrencyResponse>(validationResult);

            _brandCommands.AssignBrandCurrency(request);

            return new AssignBrandCurrencyResponse {Success = true};
        }

        protected SearchPackageResult SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var brands = _brandQueries.GetBrands().Where(x => brandFilterSelections.Contains(x.Id));

            var currencies = brands
                .SelectMany(x => x.BrandCurrencies,
                (x, c) => new CurrencyViewModel
                {
                    Code = c.Currency.Code,
                    Name = c.Currency.Name,
                    BrandId = x.Id,
                    BrandName = x.Name,
                    DefaultCurrency = x.DefaultCurrency,
                    BaseCurrency = x.BaseCurrency,
                    DateAdded = c.DateAdded,
                    AddedBy = c.AddedBy
                });

            var dataBuilder = new SearchPackageDataBuilder<CurrencyViewModel>(searchPackage, currencies);

            dataBuilder
                .Map(record => record.BrandId + "," + record.Code,
                    record =>
                        new object[]
                        {
                            record.Code,
                            record.Name,
                            record.BrandName,
                            record.DefaultCurrency,
                            record.BaseCurrency,
                            Format.FormatDate(record.DateAdded, false),
                            record.AddedBy
                        }
                );
            return dataBuilder.GetPageData(record => record.Code);
        }
    }
}