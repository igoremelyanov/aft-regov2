using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.AdminApi.Controllers.Brand
{
    [Authorize]
    public class ContentTranslationController : BaseApiController
    {
        private readonly ContentTranslationQueries _translationQueries;
        private readonly ContentTranslationCommands _translationCommands;

        public ContentTranslationController(
            ContentTranslationQueries translationQueries,
            ContentTranslationCommands translationCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _translationQueries = translationQueries;
            _translationCommands = translationCommands;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.GetContentTranslations)]
        public IHttpActionResult GetContentTranslations([FromUri] SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<ContentTranslation>(searchPackage, 
                _translationQueries.GetContentTranslations());

            var languageNames = _translationQueries.GetAllCultureCodes().ToDictionary(c => c.Code, c => c.Name);

            dataBuilder
                .Map(ct => ct.Id,
                    ct =>
                        new[]
                        {
                            ct.Name,
                            languageNames[ct.Language],
                            ct.Source,
                            ct.Translation,
                            ct.Status.ToString(),
                            ct.CreatedBy,
                            Format.FormatDate(ct.Created, false),
                            ct.Remark,
                            ct.UpdatedBy,
                            Format.FormatDate(ct.Updated, false),
                            ct.ActivatedBy,
                            Format.FormatDate(ct.Activated, false),
                            ct.DeactivatedBy,
                            Format.FormatDate(ct.Deactivated, false),
                        }
                );
            return Ok(dataBuilder.GetPageData(ct => ct.Name));
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateContentTranslation)]
        public IHttpActionResult CreateContentTranslation(AddContentTranslationModel model)
        {
            VerifyPermission(Permissions.Create, Modules.TranslationManager);

            IList<ValidationFailure> validationErrors = null;
            var createTranslationData = Mapper.DynamicMap<AddContentTranslationData>(model);

            foreach (var translationData in model.Translations)
            {
                createTranslationData.Language = translationData.Language;
                createTranslationData.Translation = translationData.Translation;

                var validationResult = _translationCommands.ValidateThatContentTranslationCanBeAdded(createTranslationData);
                if (!validationResult.IsValid)
                {
                    validationErrors = validationResult.Errors;
                    break;
                }

                _translationCommands.CreateContentTranslation(createTranslationData);
            }

            return Ok(validationErrors != null ? ValidationExceptionResponse(validationErrors) : new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateContentTranslation)]
        public IHttpActionResult UpdateContentTranslation(EditContentTranslationData data)
        {
            VerifyPermission(Permissions.Update, Modules.TranslationManager);

            var validationResult = _translationCommands.ValidateThatContentTranslationCanBeEdited(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _translationCommands.UpdateContentTranslation(data);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivateContentTranslation)]
        public IHttpActionResult Activate(ActivateContentTranslationData data)
        {
            VerifyPermission(Permissions.Activate, Modules.TranslationManager);

            _translationCommands.ActivateContentTranslation(data.Id, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivateContentTranslation)]
        public IHttpActionResult Deactivate(DeactivateContentTranslationData data)
        {
            VerifyPermission(Permissions.Deactivate, Modules.TranslationManager);

            _translationCommands.DeactivateContentTranslation(data.Id, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeleteContentTranslation)]
        public IHttpActionResult DeleteContentTranslation(DeleteContentTranslationData data)
        {
            VerifyPermission(Permissions.Delete, Modules.TranslationManager);

            if (_translationQueries.GetContentTranslation(data.Id) == null)
                return Ok(new { result = "failed" });

            _translationCommands.DeleteContentTranslation(data.Id);
            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetContentTranslationAddData)]
        public IHttpActionResult GetContentTranslationAddData()
        {
            var languages = _translationQueries.GetAllCultureCodes().Where(c => c.Status == CultureStatus.Active).ToArray();

            return Ok(new
            {
                languages = languages.Select(l => new
                {
                    l.Code,
                    l.Name,
                    l.NativeName
                })
            });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetContentTranslationEditData)]
        public IHttpActionResult GetContentTranslationEditData(Guid id)
        {
            var contentTranslation = _translationQueries.GetContentTranslation(id);

            var languages = _translationQueries.GetAllCultureCodes().Where(c => c.Status == CultureStatus.Active).ToArray();

            return Ok(new
            {
                data = new
                {
                    contentTranslation.Id,
                    contentTranslation.Name,
                    contentTranslation.Source,
                    contentTranslation.Language,
                    contentTranslation.Translation,
                    contentTranslation.Remark
                },
                languages = languages.Select(l => new
                {
                    l.Code,
                    l.Name,
                    l.NativeName
                })
            });
        }
    }
}