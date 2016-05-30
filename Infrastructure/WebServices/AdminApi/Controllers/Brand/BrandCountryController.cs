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
    public class CountryViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }

    [Authorize]
    public class BrandCountryController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly IAdminQueries _adminQueries;

        public BrandCountryController(
            BrandQueries queries, 
            BrandCommands commands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _brandQueries = queries;
            _brandCommands = commands;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Route(AdminApiRoutes.ListBrandCountries)]
        [Filters.SearchPackageFilter("searchPackage")]
        public SearchPackageResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.SupportedCountries);
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandCountryAssignData)]
        public BrandCountryAssignDataResponse GetAssignData(Guid brandId)
        {
            VerifyPermission(Permissions.Create, Modules.SupportedCountries);
            CheckBrand(brandId);

            var brand = _brandQueries.GetBrandOrNull(brandId);
            var allowedCountries = _brandQueries.GetAllowedCountriesByBrand(brandId);
            var assignedCountries = _brandQueries.GetCountriesByBrand(brandId).ToArray();
            var availableCountries = allowedCountries.Except(assignedCountries);

            return new BrandCountryAssignDataResponse
            {
                AvailableCountries = availableCountries.Select(x => new Country {Code = x.Code, Name = x.Name}),
                AssignedCountries = assignedCountries.Select(x => new Country {Code = x.Code, Name = x.Name}),
                IsActive = brand.Status == BrandStatus.Active
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.AssignBrandCountry)]
        public AssignBrandCountryResponse Assign(AssignBrandCountryRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.SupportedCountries);
            CheckBrand(request.Brand);

            var validationResult = _brandCommands.ValidateThatBrandCountryCanBeAssigned(request);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<AssignBrandCountryResponse>(validationResult);

            _brandCommands.AssignBrandCountry(request);
            return new AssignBrandCountryResponse { Success = true };
        }

        protected SearchPackageResult SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var brandCountries = _brandQueries.GetAllBrandCountries()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var countries = brandCountries.Select(bc => new CountryViewModel
            {
                Code = bc.Country.Code,
                Name = bc.Country.Name,
                BrandId = bc.BrandId,
                BrandName = bc.Brand.Name,
                DateAdded = bc.DateAdded,
                AddedBy = bc.AddedBy
            }).AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<CountryViewModel>(searchPackage, countries);

            dataBuilder.Map(record => record.BrandId + "," + record.Code, record => new[]
            {
                record.Code,
                record.Name,
                record.BrandName,
                Format.FormatDate(record.DateAdded, false),
                record.AddedBy
            });

            return dataBuilder.GetPageData(record => record.Code);
        }
    }
}