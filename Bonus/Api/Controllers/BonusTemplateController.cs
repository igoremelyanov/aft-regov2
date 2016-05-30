using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Shared.ApiDataFiltering;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class BonusTemplateController : BaseApiController
    {
        private readonly BonusManagementCommands _bonusCommands;
        private readonly BonusQueries _bonusQueries;

        public BonusTemplateController(
            BonusManagementCommands bonusCommands,
            BonusQueries bonusQueries)
        {
            _bonusCommands = bonusCommands;
            _bonusQueries = bonusQueries;
        }

        [HttpPost]
        [Route(Routes.ListBonusTemplates)]
        public FilteredDataResponse<TemplateSummary> Data(BrandFilteredDataRequest request)
        {
            var queryable = _bonusQueries
                .GetTemplates()
                .Where(t => request.BrandFilters.Contains(t.Info.BrandId));

            var dataBuilder = new FilteredDataBuilder<Core.Models.Data.Template>(request.DataRequest, queryable);
            var templateResult = dataBuilder.GetPageData();
            var bonuses = _bonusQueries.GetBonuses().ToList();

            return new FilteredDataResponse<TemplateSummary>
            {
                Page = templateResult.Page,
                Records = templateResult.Records,
                Total = templateResult.Total,
                Rows = ConvertToTemplateSummaryRows(templateResult.Rows, bonuses).ToList()
            };
        }

        private IEnumerable<TemplateSummary> ConvertToTemplateSummaryRows(List<Core.Models.Data.Template> templates, List<Core.Models.Data.Bonus> bonuses)
        {
            foreach (var template in templates)
            {
                var usedInBonuses = bonuses.Where(a => a.TemplateId == template.Id);
                var canBeEdited = usedInBonuses.All(b => b.IsActive == false);
                var canBeDeleted = usedInBonuses.Any() == false;

                yield return new TemplateSummary
                {
                    Id = template.Id,
                    Name = template.Info.Name,
                    BrandId = template.Info.BrandId,
                    Mode = template.Info.Mode,
                    Type = template.Info.TemplateType,
                    Status = template.Status,
                    CreatedBy = template.CreatedBy,
                    CreatedOn = template.CreatedOn,
                    UpdatedBy = template.UpdatedBy,
                    UpdatedOn = template.UpdatedOn,
                    CanBeEdited = canBeEdited,
                    CanBeDeleted = canBeDeleted
                };
            }
        }

        [HttpGet]
        [Route(Routes.GetTemplateOrNull)]
        public Core.Models.Data.Template GetTemplateOrNull(Guid templateId)
        {
            return _bonusQueries.GetTemplateOrNull(templateId);
        }

        [HttpGet]
        [Route(Routes.GetCompletedTemplates)]
        public List<BonusTemplate> GetCompletedTemplates()
        {
            var bonusTemplates = _bonusQueries
                .GetTemplates()
                .Where(template => template.Status == TemplateStatus.Complete)
                .Select(template => new BonusTemplate
                {
                    Id = template.Id,
                    Name = template.Info.Name,
                    BrandId = template.Info.BrandId,
                    RequireBonusCode = template.Info.Mode == IssuanceMode.AutomaticWithCode
                })
                .ToList();

            return bonusTemplates;
        }

        [HttpPost]
        [Route(Routes.CreateEditBonusTemplate)]
        public AddEditTemplateResponse CreateEdit(CreateUpdateTemplate model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<AddEditTemplateResponse>(validationResult);

            var identifier = _bonusCommands.AddUpdateTemplate(model);

            return new AddEditTemplateResponse { Success = true, Id = identifier.Id, Version = identifier.Version };
        }

        [HttpPost]
        [Route(Routes.DeleteBonusTemplate)]
        public DeleteTemplateResponse Delete(DeleteTemplate model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponse<DeleteTemplateResponse>(validationResult);

            _bonusCommands.DeleteTemplate(model);
            return new DeleteTemplateResponse { Success = true };
        }
    }
}