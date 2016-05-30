using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class DepositHistoryRequest
    {
    }

    public class DepositHistoryResponse
    {
        public IEnumerable<OfflineDeposit> Deposits { get; set; }
        public int TotalItemsCount { get; set; }
        public int PageSize { get; set; }
    }
}
