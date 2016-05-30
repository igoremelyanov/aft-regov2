using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands
{
    public class AddMessageTemplate : BaseMessageTemplate
    {
        public Guid BrandId { get; set; }
        public string LanguageCode { get; set; }
        public MessageType MessageType { get; set; }
        public MessageDeliveryMethod MessageDeliveryMethod { get; set; }
    }
}