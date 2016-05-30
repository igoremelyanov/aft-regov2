using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    /// <summary>
    /// This DTO class will contain properties that are configurable by UI elements on Bonus Creation screen
    /// </summary>
    public class CreateUpdateBonus
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime ActiveTo { get; set; }
        public string Description { get; set; }
        public DurationType DurationType { get; set; }
        public DateTime DurationStart { get; set; }
        public DateTime DurationEnd { get; set; }
        public int DaysToClaim { get; set; }

        public int DurationDays { get; set; }
        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }
    }
}