using System;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class ClaimRedemptionRequest 
    {
        public Guid RedemptionId { get; set; }
    }

    public class ClaimRedemptionResponse
    {
        public string UriToClaimedRedemption { get; set; }
    }
}