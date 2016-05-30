using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminApi.Interface.Bonus
{
    public class BonusDataResponse
    {
        public Bonus Bonus { get; set; }
        public List<BonusTemplate> Templates { get; set; }
    }

    public class Bonus
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid TemplateId { get; set; }
        public string Type { get; set; }
        public string Mode { get; set; }
        public string LicenseeName { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ActiveFrom { get; set; }
        public string ActiveTo { get; set; }
        public string Description { get; set; }

        public string DurationType { get; set; }
        public string DurationStart { get; set; }
        public string DurationEnd { get; set; }
        public string DurationDays { get; set; }
        public string DurationHours { get; set; }
        public string DurationMinutes { get; set; }

        public int DaysToClaim { get; set; }
    }

    public class BonusTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LicenseeId { get; set; }
        public string LicenseeName { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public bool RequireBonusCode { get; set; }
    }
}
