using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.Core.Messaging.Interface.Events
{
    public class MessageTemplateAddedEvent : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string LanguageCode { get; set; }
        public MessageType MessageType { get; set; }
        public MessageDeliveryMethod MessageDeliveryMethod { get; set; }
        public string TemplateName { get; set; }
        public string MessageContent { get; set; }
        public string MessageSubject { get; set; }
        public Status Status { get; set; }
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }

        public MessageTemplateAddedEvent(){}

        public MessageTemplateAddedEvent(MessageTemplate messageTemplate)
        {
            Id = messageTemplate.Id;
            BrandId = messageTemplate.BrandId;
            LanguageCode = messageTemplate.LanguageCode;
            MessageType = messageTemplate.MessageType;
            MessageDeliveryMethod = messageTemplate.MessageDeliveryMethod;
            TemplateName = messageTemplate.TemplateName;
            MessageContent = messageTemplate.MessageContent;
            MessageSubject = messageTemplate.Subject;
            Status = messageTemplate.Status;
            Created = messageTemplate.Created;
            CreatedBy = messageTemplate.CreatedBy;
        }
    }
}