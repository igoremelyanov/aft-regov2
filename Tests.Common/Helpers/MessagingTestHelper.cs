using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using AutoMapper;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class MessagingTestHelper
    {
        private readonly IMessagingRepository _messagingRepository;

        static MessagingTestHelper()
        {
            Mapper.CreateMap<AddMessageTemplate, EditMessageTemplate>();
        }

        public MessagingTestHelper(IMessagingRepository messagingRepository)
        {
            _messagingRepository = messagingRepository;
        }

        public AddMessageTemplate CreateAddMessageTemplateData(
            Guid? brandId = null,
            string cultureCode = null,
            MessageType? messageType = null,
            MessageDeliveryMethod? messageDeliveryMethod = null,
            string content = null,
            string subject = null)
        {
            var brand = brandId.HasValue
                ? _messagingRepository.Brands.Include(x => x.Languages).Single(x => x.Id == brandId.Value)
                : _messagingRepository.Brands.Include(x => x.Languages).First();

            messageDeliveryMethod = messageDeliveryMethod ?? TestDataGenerator.GetRandomMessageDeliveryMethod();

            var addMessageTemplateData = new AddMessageTemplate
            {
                BrandId = brand.Id,
                LanguageCode = cultureCode ?? brand.Languages.First().Code,
                MessageType = messageType ?? TestDataGenerator.GetRandomMessageType(),
                MessageDeliveryMethod = messageDeliveryMethod.Value,
                TemplateName = TestDataGenerator.GetRandomString(),
                Subject = messageDeliveryMethod == MessageDeliveryMethod.Email 
                    ? subject ?? TestDataGenerator.GetRandomString()
                    : null
            };

            addMessageTemplateData.MessageContent = content ??
                string.Format("Test Message - {0} {1}",
                    addMessageTemplateData.MessageType,
                    addMessageTemplateData.MessageDeliveryMethod);

            return addMessageTemplateData;
        }

        public EditMessageTemplate CreateEditMessageTemplateData(MessageDeliveryMethod? messageDeliveryMethod = null)
        {
            var messageTemplate = messageDeliveryMethod == null
                ? _messagingRepository.MessageTemplates.First()
                : _messagingRepository.MessageTemplates.First(x => x.MessageDeliveryMethod == messageDeliveryMethod);

            var editMessageTemplateData = new EditMessageTemplate
            {
                Id = messageTemplate.Id,
                TemplateName = TestDataGenerator.GetRandomString(),
                Subject = messageTemplate.MessageDeliveryMethod == MessageDeliveryMethod.Email
                    ? TestDataGenerator.GetRandomString()
                    : null,
                MessageContent = TestDataGenerator.GetRandomString()
            };

            return editMessageTemplateData;
        }
    }
}