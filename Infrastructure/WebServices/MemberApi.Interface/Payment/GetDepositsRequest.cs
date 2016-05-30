using System;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class GetDepositsRequest
    {
        public int Page { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DepositType? DepositType { get; set; }
    }
}
