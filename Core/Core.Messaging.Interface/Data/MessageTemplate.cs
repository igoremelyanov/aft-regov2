using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Messaging.Interface.Data
{
    public class MessageTemplate
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public MessageType MessageType { get; set; }
        public MessageDeliveryMethod MessageDeliveryMethod { get; set; }
        public string TemplateName { get; set; }
        public string MessageContent { get; set; }
        public string Subject { get; set; }
        public Status Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset? Activated { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? Deactivated { get; set; }
        public string Remarks { get; set; }
    }
}