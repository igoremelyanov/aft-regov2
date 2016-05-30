using System;
using System.Collections.Generic;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CreateUpdateTemplateAvailability
    {
        public CreateUpdateTemplateAvailability()
        {
            VipLevels = new List<string>();
            ExcludeBonuses = new List<Guid>();
            ExcludeRiskLevels = new List<Guid>();
        }

        public Guid? ParentBonusId { get; set; }
        public DateTime? PlayerRegistrationDateFrom { get; set; }
        public DateTime? PlayerRegistrationDateTo { get; set; }
        public int WithinRegistrationDays { get; set; }
        public List<string> VipLevels { get; set; }
        public Operation ExcludeOperation { get; set; }
        public List<Guid> ExcludeBonuses { get; set; }
        public List<Guid> ExcludeRiskLevels { get; set; }
        public int PlayerRedemptionsLimit { get; set; }
        public BonusPlayerRedemptionsLimitType PlayerRedemptionsLimitType { get; set; }
        public int RedemptionsLimit { get; set; }
    }
}