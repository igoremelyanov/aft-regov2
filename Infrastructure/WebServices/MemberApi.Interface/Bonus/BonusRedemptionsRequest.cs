using System;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class BonusRedemptionsRequest
    {
    }

    public class BonusRedemptionsResponse
    {
        public ClaimableRedemption[] Redemptions { get; set; }
    }

    public class ClaimableRedemption
    {
        public Guid Id { get; set; }
        public string BonusName { get; set; }
        public decimal Amount { get; set; }
        public string Percentage { get; set; }
        public int State { get; set; }
        public string ClaimableFrom { get; set; }
        public string ClaimableTo { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
    }

    public enum ClaimableRedemptionState
    {
        Active,
        Expired
    }
}