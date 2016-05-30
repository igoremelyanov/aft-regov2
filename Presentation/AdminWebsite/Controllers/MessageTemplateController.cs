using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels.Messaging;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class MessageTemplateController : BaseController
    {
        private readonly IAdminQueries _adminQueries;
        private readonly IMessageTemplateCommands _messageTemplateCommands;
        private readonly IMessageTemplateQueries _messageTemplateQueries;
        private readonly BrandQueries _brandQueries;

        static MessageTemplateController()
        {
            Mapper.CreateMap<AddMessageTemplateModel, AddMessageTemplate>();
            Mapper.CreateMap<EditMessageTemplateModel, EditMessageTemplate>();
            Mapper.CreateMap<ActivateMessageTemplateModel, ActivateMessageTemplate>();
        }

        public MessageTemplateController(
            IAdminQueries adminQueries,
            IMessageTemplateCommands messageTemplateCommands,
            IMessageTemplateQueries messageTemplateQueries,
            BrandQueries brandQueries)
        {
            _adminQueries = adminQueries;
            _messageTemplateCommands = messageTemplateCommands;
            _messageTemplateQueries = messageTemplateQueries;
            _brandQueries = brandQueries;
        }

        public ActionResult Help()
        {
            return View();
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var ax = _messageTemplateQueries.GetMessageTemplates().ToArray();

            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var allowedBrands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands().Where(x => brandFilterSelections.Contains(x.Id)), CurrentUser.Id)
                .Select(x => x.Id)
                .ToArray();

            var templates = _messageTemplateQueries.GetMessageTemplates()
                .Where(x => allowedBrands.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<MessageTemplate>(searchPackage, templates);

            dataBuilder.Map(template => template.Id, template => new[]
            {
                template.BrandName,
                template.LanguageName,
                Enum.GetName(typeof(MessageType), template.MessageType),
                Enum.GetName(typeof(MessageDeliveryMethod), template.MessageDeliveryMethod),
                template.TemplateName,
                Enum.GetName(typeof(Status), template.Status),
                template.Created.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                template.CreatedBy,
                template.Updated.HasValue ? template.Updated.Value.ToString("yyyy/MM/dd HH:mm:ss zzz") : "",
                template.UpdatedBy,
                template.Activated.HasValue ? template.Activated.Value.ToString("yyyy/MM/dd HH:mm:ss zzz") : "",
                template.ActivatedBy,
                template.Deactivated.HasValue ? template.Deactivated.Value.ToString("yyyy/MM/dd HH:mm:ss zzz") : "",
                template.DeactivatedBy
            });

            return new JsonResult
            {
                Data = dataBuilder.GetPageData(record => record.BrandName),
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public string Add()
        {
            var selectedLicensees = _adminQueries.GetLicenseeFilterSelections();

            var licensees = _brandQueries
                .GetFilteredLicensees()
                .Where(x => selectedLicensees.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name});

            return SerializeJson(new
            {
                licensees,
                messageTypes = Enum.GetNames(typeof(MessageType)).OrderBy(x => x),
                messageDeliveryMethods = Enum.GetNames(typeof(MessageDeliveryMethod)).OrderBy(x => x)
            });
        }

        public string Brands(Guid licenseeId)
        {
            var selectedBrands = _adminQueries.GetBrandFilterSelections();

            var brands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.Id)
                .Where(x =>
                    selectedBrands.Contains(x.Id) &&
                    x.LicenseeId == licenseeId)
                .OrderBy(x => x.Name)
                .Select(x => new {x.Id, x.Name});

            return SerializeJson(new {brands});
        }

        public string Languages(Guid brandId)
        {
            var languages = _messageTemplateQueries
                .GetBrandLanguages(brandId)
                .OrderBy(x => x.Name);

            return SerializeJson(new {languages});
        }

        [HttpPost]
        public ActionResult Add(AddMessageTemplateModel model)
        {
            try
            {
                var data = Mapper.Map<AddMessageTemplate>(model);
                var validationResult = _messageTemplateQueries.GetValidationResult(data);

                if (!validationResult.IsValid)
                    return ValidationErrorResponseActionResult(validationResult.Errors);

                var id = _messageTemplateCommands.Add(data);

                return this.Success(id);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public string Edit(Guid id)
        {
            var allowedBrands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.Id)
                .Select(x => x.Id)
                .ToArray();

            var template = _messageTemplateQueries.GetMessageTemplates().SingleOrDefault(x =>
                x.Id == id &&
                allowedBrands.Contains(x.BrandId));

            if (template == null)
                return null;

            var brand = _brandQueries.GetBrand(template.BrandId);

            return SerializeJson(new
            {
                licenseeName = _brandQueries.GetLicensee(brand.LicenseeId).Name,
                brandName = brand.Name,
                languageName = template.LanguageName,
                messageType = Enum.GetName(typeof (MessageType), template.MessageType),
                messageDeliveryMethod = Enum.GetName(typeof (MessageDeliveryMethod), template.MessageDeliveryMethod),
                templateName = template.TemplateName,
                subject = template.Subject,
                messageContent = template.MessageContent
            });
        }

        [HttpPost]
        public ActionResult Edit(EditMessageTemplateModel model)
        {
            try
            {
                var data = Mapper.Map<EditMessageTemplate>(model);

                var validationResult = _messageTemplateQueries.GetValidationResult(data);

                if (!validationResult.IsValid)
                    return ValidationErrorResponseActionResult(validationResult.Errors);

                _messageTemplateCommands.Edit(data);

                return this.Success();
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpPost]
        public ActionResult Activate(ActivateMessageTemplateModel model)
        {
            try
            {
                var data = Mapper.Map<ActivateMessageTemplate>(model);
                var validationResult = _messageTemplateQueries.GetValidationResult(data);

                if (!validationResult.IsValid)
                    return ValidationErrorResponseActionResult(validationResult.Errors);

                _messageTemplateCommands.Activate(data);

                return this.Success();
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public string View(Guid id)
        {
            var allowedBrands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.Id)
                .Select(x => x.Id)
                .ToArray();

            var template = _messageTemplateQueries.GetMessageTemplates().SingleOrDefault(x =>
                x.Id == id &&
                allowedBrands.Contains(x.BrandId));

            if (template == null)
                return null;

            var brand = _brandQueries.GetBrand(template.BrandId);

            return SerializeJson(new
            {
                licenseeName = _brandQueries.GetLicensee(brand.LicenseeId).Name,
                brandName = brand.Name,
                languageName = template.LanguageName,
                messageType = Enum.GetName(typeof(MessageType), template.MessageType),
                messageDeliveryMethod = Enum.GetName(typeof(MessageDeliveryMethod), template.MessageDeliveryMethod),
                templateName = template.TemplateName,
                subject = template.Subject,
                messageContent = template.MessageContent
            });
        }
    }
}