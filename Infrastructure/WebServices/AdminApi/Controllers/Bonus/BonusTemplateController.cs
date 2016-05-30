using System;
using System.Data.Entity;
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
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AutoMapper;
using Game = AFT.RegoV2.AdminApi.Interface.Bonus.Game;
using Licensee = AFT.RegoV2.AdminApi.Interface.Bonus.Licensee;
using SearchPackageResult = AFT.RegoV2.AdminWebsite.Common.jqGrid.SearchPackageResult;
using TemplateBrand = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateBrand;
using TemplateCurrency = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateCurrency;
using TemplateDataResponse = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateDataResponse;
using TemplateRiskLevel = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateRiskLevel;
using TemplateVipLevel = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateVipLevel;
using TemplateWalletTemplate = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateWalletTemplate;

namespace AFT.RegoV2.AdminApi.Controllers.Bonus
{
    [Authorize]
    [DisableCamelCaseSerialization]
    public class BonusTemplateController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly IAdminQueries _adminQueries;
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly IGameQueries _gameQueries;
        private readonly IRiskLevelQueries _riskLevelQueries;
        private readonly IMessageTemplateQueries _messageTemplateQueries;

        public BonusTemplateController(
            BrandQueries brandQueries,
            IGameQueries gameQueries,
            IRiskLevelQueries riskLevelQueries,
            IMessageTemplateQueries messageTemplateQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries,
            IBonusApiProxy bonusApiProxy)
            : base(authQueries, adminQueries)
        {
            _brandQueries = brandQueries;
            _adminQueries = adminQueries;
            _bonusApiProxy = bonusApiProxy;
            _gameQueries = gameQueries;
            _riskLevelQueries = riskLevelQueries;
            _messageTemplateQueries = messageTemplateQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListBonusTemplates)]
        public async Task<SearchPackageResult> Data([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.BonusTemplateManager);
            VerifyPermission(Permissions.View, Modules.BonusManager);

            var request = Mapper.Map<FilteredDataRequest>(searchPackage);
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections().ToList();
            var filteredTemplates = await _bonusApiProxy.GetFilteredBonusTemplatesAsync(new BrandFilteredDataRequest
            {
                DataRequest = request,
                BrandFilters = brandFilterSelections
            });
            var licensees = _brandQueries.GetAllLicensees().Select(l => new { l.Name, Brands = l.Brands.Select(b => new { b.Id, b.Name })}).ToList();
            var searchPackageResult = new SearchPackageResult
            {
                page = filteredTemplates.Page,
                total = filteredTemplates.Total,
                records = filteredTemplates.Records,
                rows = filteredTemplates.Rows.Select(template => new SearchPackageResultRow
                {
                    id = template.Id,
                    cell = new object[]
                    {
                        template.Name,
                        licensees.Single(l => l.Brands.Select(b => b.Id).Contains(template.BrandId)).Name,
                        licensees.SelectMany(l => l.Brands).Single(b => b.Id == template.BrandId).Name,
                        template.Mode.ToString(),
                        template.Type,
                        template.Status,
                        template.CreatedBy,
                        Format.FormatDate(template.CreatedOn),
                        template.UpdatedBy,
                        Format.FormatDate(template.UpdatedOn),
                        template.CanBeEdited,
                        template.CanBeDeleted
                    }
                }).ToList()
            };

            return searchPackageResult;
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetBonusTemplateRelatedData)]
        public async Task<TemplateDataResponse> GetRelatedData(Guid? id = null)
        {
            VerifyPermission(Permissions.View, Modules.BonusTemplateManager);
            VerifyPermission(Permissions.View, Modules.BonusManager);

            Template template = null;
            if (id.HasValue)
            {
                var templateData = await _bonusApiProxy.GetTemplateOrNull(id.Value);
                template = Mapper.Map<Template>(templateData);
                CheckBrand(template.Info.BrandId.Value);
            }

            var getBonusesTask = _bonusApiProxy.GetBonusesAsync();

            var notificationTriggers = _messageTemplateQueries.GetBonusNotificationTriggerMessageTypes()
                .Select(x => Enum.GetName(typeof(MessageType), x));

            var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var licensees = _brandQueries.GetFilteredLicensees()
                .Include(l => l.Brands.Select(b => b.VipLevels))
                .Include(l => l.Brands.Select(b => b.BrandCurrencies.Select(bc => bc.Currency)))
                .Include(l => l.Brands.Select(b => b.WalletTemplates))
                .Include(l => l.Brands.Select(b => b.Products))
                .Where(l => licenseeFilterSelections.Contains(l.Id))
                .OrderBy(l => l.Name)
                .ToList()
                .Select(l => new Licensee
                {
                    Id = l.Id,
                    Name = l.Name,
                    Brands = _brandQueries.GetFilteredBrands(l.Brands, UserId)
                        .Where(b =>
                            b.Status == BrandStatus.Active &&
                            brandFilterSelections.Contains(b.Id))
                        .OrderBy(b => b.Name)
                        .Select(b => new TemplateBrand
                        {
                            Id = b.Id,
                            Name = b.Name,
                            VipLevels = b.VipLevels.Select(v => new TemplateVipLevel { Code = v.Code, Name = v.Name }),
                            Currencies = b.BrandCurrencies.Select(v => new TemplateCurrency { Code = v.Currency.Code, Name = v.Currency.Name }),
                            WalletTemplates = b.WalletTemplates.OrderBy(wt => wt.Name).Select(wt => new TemplateWalletTemplate { Id = wt.Id, Name = wt.Name, IsMain = wt.IsMain }),
                            Products = b.Products.Select(p => p.ProductId),
                            RiskLevels = _riskLevelQueries.GetByBrand(b.Id)
                                .Select(riskLevel => new TemplateRiskLevel { Id = riskLevel.Id, Name = riskLevel.Name })
                                .OrderBy(riskLevel => riskLevel.Name)
                        })
                }).ToList();

            var products = _gameQueries.GetGameProviders().ToList();
            var games =
                _gameQueries.GetGameDtos()
                    .Select(g => new Game { Id = g.Id.Value, Name = g.Name, ProductId = g.ProductId, ProductName = products.Single(p => p.Id == g.ProductId).Name })
                    .ToList();

            if (template != null)
            {
                if (template.Info != null)
                {
                    template.Info.LicenseeId = licensees.Single(l => l.Brands.Select(b => b.Id).Contains(template.Info.BrandId.Value)).Id;
                    template.Info.LicenseeName = licensees.Single(l => l.Brands.Select(b => b.Id).Contains(template.Info.BrandId.Value)).Name;
                    template.Info.BrandName = licensees.SelectMany(l => l.Brands).Single(b => b.Id == template.Info.BrandId).Name;
                }

                if (template.Wagering != null)
                {
                    foreach (var gc in template.Wagering.GameContributions)
                    {
                        var game = games.Single(g => g.Id == gc.GameId);
                        gc.Name = game.Name;
                        gc.ProductId = game.ProductId;
                        gc.ProductName = game.ProductName;
                    }
                }
            }

            var bonuses = await getBonusesTask;
            bonuses = bonuses.Where(b => brandFilterSelections.Contains(b.BrandId)).OrderBy(bonus => bonus.Name).ToList();

            return new TemplateDataResponse
            {
                Template = template,
                NotificationTriggers = notificationTriggers,
                Bonuses = bonuses,
                Licensees = licensees,
                Games = games
            };
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateEditBonusTemplate)]
        public async Task<AddEditTemplateResponse> CreateEdit(CreateUpdateTemplate model)
        {
            VerifyPermission(Permissions.Create, Modules.BonusTemplateManager);
            VerifyPermission(Permissions.Update, Modules.BonusTemplateManager);

            var brandId = model.Id == Guid.Empty ? 
                model.Info.BrandId : 
                (await _bonusApiProxy.GetTemplateOrNull(model.Id)).Info.BrandId;
            CheckBrand(brandId.Value);

            return await _bonusApiProxy.CreateUpdateBonusTemplateAsync(model);
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeleteBonusTemplate)]
        public async Task<DeleteTemplateResponse> Delete(DeleteTemplate model)
        {
            VerifyPermission(Permissions.Delete, Modules.BonusTemplateManager);

            var template = await _bonusApiProxy.GetTemplateOrNull(model.TemplateId);
            if(template != null)
                CheckBrand(template.Info.BrandId);

            return await _bonusApiProxy.DeleteBonusTemplateAsync(model);
        }
    }
}