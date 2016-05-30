using System;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class CompleteBonusesRequest
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
