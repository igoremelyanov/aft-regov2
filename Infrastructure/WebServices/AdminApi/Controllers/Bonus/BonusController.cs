using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Attributes;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Bonus;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AutoMapper;
using BonusTemplate = AFT.RegoV2.AdminApi.Interface.Bonus.BonusTemplate;
using SearchPackageResult = AFT.RegoV2.AdminWebsite.Common.jqGrid.SearchPackageResult;

namespace AFT.RegoV2.AdminApi.Controllers.Bonus
{
    [Authorize]
    [DisableCamelCaseSerialization]
    public class BonusController : BaseApiController
    {
        private readonly IAdminQueries _adminQueries;
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly BrandQueries _brandQueries;

        public BonusController(
            IAuthQueries authQueries,
            IAdminQueries adminQueries,
            IBonusApiProxy bonusApiProxy,
            BrandQueries brandQueries)
            : base(authQueries, adminQueries)
        {
            _adminQueries = adminQueries;
            _bonusApiProxy = bonusApiProxy;
            _brandQueries = brandQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListBonuses)]
        public async Task<SearchPackageResult> Data([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.BonusManager);

            var request = Mapper.Map<FilteredDataRequest>(searchPackage);
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections().ToList();
            var filteredBonuses = await _bonusApiProxy.GetFilteredBonusesAsync(new BrandFilteredDataRequest
            {
                BrandFilters = brandFilterSelections,
                DataRequest = request
            });
            var licensees = _brandQueries.GetAllLicensees().Select(l => new { l.Name, Brands = l.Brands.Select(b => new { b.Id, b.Name }) }).ToList();
            var searchPackageResult = new SearchPackageResult
            {
                page = filteredBonuses.Page,
                total = filteredBonuses.Total,
                records = filteredBonuses.Records,
                rows = filteredBonuses.Rows.Select(bonus => new SearchPackageResultRow
                {
                    id = bonus.Id,
                    cell = new object[]
                    {
                        bonus.Name,
                        bonus.Code,
                        bonus.Type.ToString(),
                        bonus.Mode.ToString(),
                        Format.FormatDate(bonus.ActiveFrom, false),
                        Format.FormatDate(bonus.ActiveTo, false),
                        licensees.Single(l => l.Brands.Select(b => b.Id).Contains(bonus.BrandId)).Name,
                        licensees.SelectMany(l => l.Brands).Single(b => b.Id == bonus.BrandId).Name,
                        bonus.IsActive,
                        bonus.CreatedBy,
                        Format.FormatDate(bonus.CreatedOn),
                        bonus.UpdatedBy,
                        Format.FormatDate(bonus.UpdatedOn)
                    }
                }).ToList()
            };

            return searchPackageResult;
        }

        [HttpPost]
        [Route(AdminApiRoutes.ChangeBonusStatus)]
        public async Task<ToggleBonusStatusResponse> ChangeStatus(ToggleBonusStatus model)
        {
            VerifyPermission(Permissions.Activate, Modules.BonusManager);
            VerifyPermission(Permissions.Deactivate, Modules.BonusManager);

            var bonus = await _bonusApiProxy.GetBonusOrNull(model.Id);
            if (bonus != null)
                CheckBrand(bonus.BrandId);

            return await _bonusApiProxy.ChangeBonusStatusAsync(model);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBonusRelatedData)]
        public async Task<BonusDataResponse> GetRelatedData(Guid? id = null)
        {
            VerifyPermission(Permissions.View, Modules.BonusTemplateManager);
            VerifyPermission(Permissions.View, Modules.BonusManager);

            var templatesTask = _bonusApiProxy.GetCompletedTemplatesAsync();

            var licensees = _brandQueries.GetAllLicensees().Select(l => new { l.Id, l.Name, Brands = l.Brands.Select(b => new { b.Id, b.Name }) }).ToList();
            Interface.Bonus.Bonus bonus = null;
            if (id.HasValue)
            {
                var bonusData = await _bonusApiProxy.GetBonusOrNull(id.Value);
                CheckBrand(bonusData.BrandId);
                bonus = Mapper.Map<Interface.Bonus.Bonus>(bonusData);
                bonus.LicenseeName = licensees.Single(l => l.Brands.Select(b => b.Id).Contains(bonusData.BrandId)).Name;
                bonus.BrandName = licensees.SelectMany(l => l.Brands).Single(b => b.Id == bonusData.BrandId).Name;
            }

            var templates = await templatesTask;

            if (id.HasValue == false)
            {
                var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
                templates = templates.Where(x => brandFilterSelections.Contains(x.BrandId)).ToList();
            }

            var bonusTemplates = Mapper.Map<List<BonusTemplate>>(templates);
            foreach (var template in bonusTemplates)
            {
                template.LicenseeId = licensees.Single(l => l.Brands.Select(b => b.Id).Contains(template.BrandId)).Id;
                template.LicenseeName = licensees.Single(l => l.Brands.Select(b => b.Id).Contains(template.BrandId)).Name;
                template.BrandName = licensees.SelectMany(l => l.Brands).Single(b => b.Id == template.BrandId).Name;
            }

            return new BonusDataResponse
            {
                Bonus = bonus,
                Templates = bonusTemplates.OrderBy(t => t.Name).ToList()
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateUpdateBonus)]
        public async Task<AddEditBonusResponse> CreateUpdate(CreateUpdateBonus model)
        {
            VerifyPermission(Permissions.Create, Modules.BonusManager);
            VerifyPermission(Permissions.Update, Modules.BonusManager);

            var template = await _bonusApiProxy.GetTemplateOrNull(model.TemplateId);
            if (template != null)
                CheckBrand(template.Info.BrandId);

            return await _bonusApiProxy.CreateUpdateBonusAsync(model);
        }
    }
}