using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;

namespace AFT.RegoV2.Core.Messaging.Interface.Events
{
    public class MassMessageSendRequestedEvent : DomainEventBase
    {
        public SendMassMessageRequest Request { get; set; }
        public DateTimeOffset SendRequestedDate { get; set; }
        public string SendRequestedBy { get; set; }

        public MassMessageSendRequestedEvent(){}

        public MassMessageSendRequestedEvent(
            SendMassMessageRequest request, 
            DateTimeOffset sendRequestedDate, 
            string sendRequestedBy)
        {
            Request = request;
            SendRequestedDate = sendRequestedDate;
            SendRequestedBy = sendRequestedBy;
        }
    }
}