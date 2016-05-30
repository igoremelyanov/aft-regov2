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
    public class CultureViewModel
    {
        public string Code { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string Status { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }

    [Authorize]
    public class BrandCultureController : BaseApiController
    {
        private readonly BrandQueries _queries;
        private readonly BrandCommands _brandCommands;
        private readonly IAdminQueries _adminQueries;

        public BrandCultureController(
            BrandQueries queries, 
            BrandCommands brandCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _queries = queries;
            _brandCommands = brandCommands;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Route(AdminApiRoutes.ListBrandCultures)]
        [Filters.SearchPackageFilter("searchPackage")]
        public SearchPackageResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.SupportedLanguages);
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBrandCultureAssignData)]
        public BrandCultureAssignDataResponse GetAssignData(Guid brandId)
        {
            VerifyPermission(Permissions.Create, Modules.SupportedLanguages);
            CheckBrand(brandId);

            var brand = _queries.GetBrandOrNull(brandId);
            var allowedCultures = _queries.GetAllowedCulturesByBrand(brandId);
            var assignedCultures = brand.BrandCultures.Select(x => x.Culture).ToList();
            var availableCultures = allowedCultures.Except(assignedCultures);

            return new BrandCultureAssignDataResponse
            {
                AvailableCultures = availableCultures.Select(x => new Culture {Code = x.Code}),
                AssignedCultures = assignedCultures.Select(x => new Culture {Code = x.Code}),
                DefaultCulture = brand.DefaultCulture,
                IsActive = brand.Status == BrandStatus.Active
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.AssignBrandCulture)]
        //public AssignBrandCultureResponse Assign(AssignBrandCultureRequest request)
        public AssignBrandCultureResponse Assign(AssignBrandCultureRequest request)
        {
            VerifyPermission(Permissions.Create, Modules.SupportedLanguages);
            CheckBrand(request.Brand);

            var validationResult = _brandCommands.ValidateThatBrandCultureCanBeAssigned(request);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<AssignBrandCultureResponse>(validationResult);

            _brandCommands.AssignBrandCulture(request);
            return new AssignBrandCultureResponse { Success = true };
        }

        protected SearchPackageResult SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var brandCultures = _queries.GetAllBrandCultures()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var cultures = brandCultures.Select(c => new CultureViewModel
            {
                Code = c.CultureCode,
                BrandId = c.Brand.Id,
                BrandName = c.Brand.Name,
                Status = c.Brand.Status.ToString(),
                DateAdded = c.DateAdded,
                AddedBy = c.AddedBy
            }).AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<CultureViewModel>(searchPackage, cultures);

            dataBuilder.Map(record => record.BrandId + "," + record.Code, record => new object[]
            {
                record.Code,
                record.BrandName,
                record.Status,
                Format.FormatDate(record.DateAdded, false),
                record.AddedBy
            });

            return dataBuilder.GetPageData(record => record.Code);
        }
    }
}