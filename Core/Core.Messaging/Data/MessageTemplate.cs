using System;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.Core.Messaging.Data
{
    public class MessageTemplate
    {
        public MessageTemplate()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }
        public virtual Messaging.Data.Brand Brand { get; set; }
        [ForeignKey("Language")]
        public string LanguageCode { get; set; }
        public virtual Language Language { get; set; }
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