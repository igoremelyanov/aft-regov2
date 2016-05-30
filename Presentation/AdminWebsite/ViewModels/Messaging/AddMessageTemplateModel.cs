using System;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.AdminWebsite.ViewModels.Messaging
{
    public class AddMessageTemplateModel : BaseMessageTemplateModel
    {
        public Guid BrandId { get; set; }
        public string LanguageCode { get; set; }
        public MessageType MessageType { get; set; }
        public MessageDeliveryMethod MessageDeliveryMethod { get; set; }
    }
}