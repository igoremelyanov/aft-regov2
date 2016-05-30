using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class WithdrawalHistoryResponse
    {
        public IEnumerable<OfflineWithdrawal> Withdrawals { get; set; }
        public int TotalItemsCount { get; set; }
        public int PageSize { get; set; }
    }
}
