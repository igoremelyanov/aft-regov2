using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Data
{
    public class BonusRedemption
    {
        public Guid Id { get; set; }
        public BonusRedemptionBonus Bonus { get; set; }
        public ActivationStatus ActivationState { get; set; }
        public RolloverStatus RolloverState { get; set; }
        public decimal Amount { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal Rollover { get; set; }
        public decimal RolloverLeft { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset ClaimableFrom { get; set; }
        public DateTimeOffset ClaimableTo { get; set; }
        public bool IsExpired { get; set; }
    }

    public class BonusRedemptionBonus
    {
        public BonusType Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}