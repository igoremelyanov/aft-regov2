using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using AutoMapper;
using FluentValidation.Results;
using Language = AFT.RegoV2.Core.Messaging.Data.Language;
using MessageTemplate = AFT.RegoV2.Core.Messaging.Data.MessageTemplate;

namespace AFT.RegoV2.Core.Messaging.ApplicationServices
{
    public class MessageTemplateQueries : MarshalByRefObject, IMessageTemplateQueries
    {
        private readonly IMessagingRepository _messagingRepository;

        static MessageTemplateQueries()
        {
            Mapper.CreateMap<Language, Interface.Data.Language>();
            
            Mapper.CreateMap<Data.Brand, Interface.Data.Brand>();

            Mapper.CreateMap<MessageTemplate, Interface.Data.MessageTemplate>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.LanguageName, opt => opt.MapFrom(src => src.Language.Name));
        }

        public MessageTemplateQueries(IMessagingRepository messagingRepository)
        {
            _messagingRepository = messagingRepository;
        }

        [Permission(Permissions.View, Module = Modules.MessageTemplateManager)]
        public IQueryable<Interface.Data.MessageTemplate> GetMessageTemplates()
        {
            var messageTemplates = _messagingRepository.MessageTemplates
                .Include(x => x.Brand)
                .Include(x => x.Language);

            return Mapper.Map<List<Interface.Data.MessageTemplate>>(messageTemplates).AsQueryable();
        }

        public IEnumerable<Interface.Data.Language> GetBrandLanguages(Guid id)
        {
            var brandLanguages = _messagingRepository.Brands
                .Include(x => x.Languages)
                .Single(x => x.Id == id)
                .Languages;

            return Mapper.Map<List<Interface.Data.Language>>(brandLanguages);
        }

        public ValidationResult GetValidationResult(AddMessageTemplate model)
        {
            return new AddMessageTemplateValidator(_messagingRepository, this).Validate(model);
        }

        public ValidationResult GetValidationResult(EditMessageTemplate model)
        {
            return new EditMessageTemplateValidator(_messagingRepository, this).Validate(model);
        }

        public ValidationResult GetValidationResult(ActivateMessageTemplate model)
        {
            return new ActivateMessageTemplateValidator(_messagingRepository).Validate(model);
        }

        public IEnumerable<MessageType> GetBonusNotificationTriggerMessageTypes()
        {
            return new[]
            {
                MessageType.BonusIssued,
                MessageType.BonusWageringRequirement
            };
        }
    }
}