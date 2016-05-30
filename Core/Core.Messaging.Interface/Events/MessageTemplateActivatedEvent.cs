using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.Core.Messaging.Interface.Events
{
    public class MessageTemplateActivatedEvent : DomainEventBase
    {
        public Guid Id { get; set; }
        public DateTimeOffset Activated { get; set; }
        public string ActivatedBy { get; set; }
        public string Remarks { get; set; }

        public MessageTemplateActivatedEvent(){}

        public MessageTemplateActivatedEvent(MessageTemplate messageTemplate)
        {
            Id = messageTemplate.Id;
            Activated = messageTemplate.Activated.Value;
            ActivatedBy = messageTemplate.ActivatedBy;
            Remarks = messageTemplate.Remarks;
        }
    }
}