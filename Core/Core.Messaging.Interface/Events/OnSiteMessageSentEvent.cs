using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;

namespace AFT.RegoV2.Core.Messaging.Interface.Events
{
    public class OnSiteMessageSentEvent : DomainEventBase
    {
        public Guid MassMessageId { get; set; }
        public Guid PlayerId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        
        public OnSiteMessageSentEvent(){}

        public OnSiteMessageSentEvent(Guid massMessageId, Guid playerId, string subject, string content)
        {
            MassMessageId = massMessageId;
            PlayerId = playerId;
            Subject = subject;
            Content = content;
        }
    }
}