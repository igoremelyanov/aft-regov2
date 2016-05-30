using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Data
{
    public class Bonus
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid TemplateId { get; set; }
        public BonusType Type { get; set; }
        public IssuanceMode Mode { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTimeOffset ActiveFrom { get; set; }
        public DateTimeOffset ActiveTo { get; set; }
        public string Description { get; set; }

        public string DurationType { get; set; }
        public DateTimeOffset DurationStart { get; set; }
        public DateTimeOffset DurationEnd { get; set; }

        public int DaysToClaim { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
    }
}