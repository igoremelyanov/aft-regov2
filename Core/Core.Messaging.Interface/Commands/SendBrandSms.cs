using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;

namespace AFT.RegoV2.Core.Messaging.Interface.Commands
{
    public class SendBrandSms: ICommand
    {
        public string RecipientNumber { get; set; }
        public Guid BrandId { get; set; }
        public MessageType MessageType { get; set; }
        public IBrandMessageTemplateModel Model { get; set; }
    }
}