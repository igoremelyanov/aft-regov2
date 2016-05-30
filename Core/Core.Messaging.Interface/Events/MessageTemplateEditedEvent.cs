using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.Core.Messaging.Interface.Events
{
    public class MessageTemplateEditedEvent : DomainEventBase
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string MessageContent { get; set; }
        public string MessageSubject { get; set; }
        public Status Status { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string UpdatedBy { get; set; }

        public MessageTemplateEditedEvent() { }

        public MessageTemplateEditedEvent(MessageTemplate messageTemplate)
        {
            Id = messageTemplate.Id;
            TemplateName = messageTemplate.TemplateName;
            MessageContent = messageTemplate.MessageContent;
            MessageSubject = messageTemplate.Subject;
            Status = messageTemplate.Status;
            Updated = messageTemplate.Updated.Value;
            UpdatedBy = messageTemplate.UpdatedBy;
        }
    }
}