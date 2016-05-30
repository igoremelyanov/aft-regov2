using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Api.Interface.Responses
{
    public class TemplateSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public IssuanceMode Mode { get; set; }
        public string Type { get; set; }
        public TemplateStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public bool CanBeEdited { get; set; }
        public bool CanBeDeleted { get; set; }
    }

    public class BonusData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
    }

    public class AddEditTemplateResponse : ValidationResponseBase
    {
        public Guid? Id { get; set; }
        public int? Version { get; set; }
    }

    public class DeleteTemplateResponse : ValidationResponseBase
    {
    }
}