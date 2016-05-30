using System;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class BonusRedemption
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Rollover { get; set; }
        public decimal RolloverLeft { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal Reward { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ActivationState { get; set; }
    }
}
