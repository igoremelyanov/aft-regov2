using System.Collections.Generic;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CreateUpdateTemplateNotification
    {
        public CreateUpdateTemplateNotification()
        {
            EmailTriggers = new List<MessageType>();
            SmsTriggers = new List<MessageType>();
        }

        public List<MessageType> EmailTriggers { get; set; }
        public List<MessageType> SmsTriggers { get; set; }
    }
}