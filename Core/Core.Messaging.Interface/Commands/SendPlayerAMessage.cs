using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;

namespace AFT.RegoV2.Core.Messaging.Interface.Commands
{
    public class SendPlayerAMessage: ICommand
    {
        public Guid PlayerId { get; set; }
        public MessageType MessageType { get; set; }
        public MessageDeliveryMethod MessageDeliveryMethod { get; set; }
        public IPlayerMessageTemplateModel Model { get; set; }
    }
}
