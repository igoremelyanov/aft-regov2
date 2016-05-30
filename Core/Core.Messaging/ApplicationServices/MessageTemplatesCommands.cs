using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using AFT.RegoV2.Core.Messaging.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Messaging.ApplicationServices
{
    public class MessageTemplateCommands : MarshalByRefObject, IMessageTemplateCommands
    {
        private readonly IMessagingRepository _messagingRepository;
        private readonly IMessageTemplateQueries _messageTemplateQueries;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        static MessageTemplateCommands()
        {
            Mapper.CreateMap<AddMessageTemplate, MessageTemplate>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Status.Inactive));

            Mapper.CreateMap<EditMessageTemplate, MessageTemplate>();

            Mapper.CreateMap<Player, IPlayerMessageTemplateModel>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name));

            Mapper.CreateMap<MessageTemplate, Interface.Data.MessageTemplate>();
        }

        public MessageTemplateCommands(
            IMessagingRepository messagingRepository,
            IMessageTemplateQueries messageTemplateQueries,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus)
        {
            _messagingRepository = messagingRepository;
            _messageTemplateQueries = messageTemplateQueries;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Create, Module = Modules.MessageTemplateManager)]
        public Guid Add(AddMessageTemplate model)
        {
            var validationResult = _messageTemplateQueries.GetValidationResult(model);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var messageTemplate = Mapper.Map<MessageTemplate>(model);

                messageTemplate.Brand = _messagingRepository.Brands.Single(x => x.Id == model.BrandId);
                messageTemplate.Language = _messagingRepository.Languages.Single(x => x.Code == model.LanguageCode);
                messageTemplate.Created = DateTimeOffset.Now.ToBrandOffset(messageTemplate.Brand.TimezoneId);
                messageTemplate.CreatedBy = _actorInfoProvider.Actor.UserName;

                _messagingRepository.MessageTemplates.Add(messageTemplate);

                _messagingRepository.SaveChanges();

                _eventBus.Publish(new MessageTemplateAddedEvent(Mapper.Map<Interface.Data.MessageTemplate>(messageTemplate))
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(messageTemplate.Brand.TimezoneId),
                });

                scope.Complete();

                return messageTemplate.Id;
            }
        }

        [Permission(Permissions.Update, Module = Modules.MessageTemplateManager)]
        public void Edit(EditMessageTemplate model)
        {
            var validationResult = _messageTemplateQueries.GetValidationResult(model);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);
            
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var messageTemplate = _messagingRepository.MessageTemplates.Single(x => x.Id == model.Id);

                messageTemplate = Mapper.Map(model, messageTemplate);
                messageTemplate.Updated = DateTimeOffset.Now.ToBrandOffset(messageTemplate.Brand.TimezoneId);
                messageTemplate.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _messagingRepository.SaveChanges();

                _eventBus.Publish(new MessageTemplateEditedEvent(Mapper.Map<Interface.Data.MessageTemplate>(messageTemplate))
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(messageTemplate.Brand.TimezoneId),
                });

                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.MessageTemplateManager)]
        public void Activate(ActivateMessageTemplate model)
        {
            var validationResult = _messageTemplateQueries.GetValidationResult(model);

            if(!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var messageTemplate = _messagingRepository.MessageTemplates.Single(x => x.Id == model.Id);

                messageTemplate.Status = Status.Active;
                messageTemplate.Activated = DateTimeOffset.Now.ToBrandOffset(messageTemplate.Brand.TimezoneId);
                messageTemplate.ActivatedBy = _actorInfoProvider.Actor.UserName;
                messageTemplate.Remarks = model.Remarks;

                var oldMessageTemplate = _messagingRepository.MessageTemplates.SingleOrDefault(x =>
                    x.Id != messageTemplate.Id &&
                    x.BrandId == messageTemplate.BrandId &&
                    x.LanguageCode == messageTemplate.LanguageCode &&
                    x.MessageType == messageTemplate.MessageType &&
                    x.MessageDeliveryMethod == messageTemplate.MessageDeliveryMethod &&
                    x.Status == Status.Active);

                if (oldMessageTemplate != null)
                    oldMessageTemplate.Status = Status.Inactive;

                _messagingRepository.SaveChanges();

                _eventBus.Publish(new MessageTemplateActivatedEvent(Mapper.Map<Interface.Data.MessageTemplate>(messageTemplate))
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(messageTemplate.Brand.TimezoneId),
                });

                scope.Complete();
            }
        }
    }
}