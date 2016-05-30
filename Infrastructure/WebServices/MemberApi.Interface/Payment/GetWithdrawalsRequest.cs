using System;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class GetWithdrawalsRequest
    {
        public int Page { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
