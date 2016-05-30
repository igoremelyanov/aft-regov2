using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class TransactionHistoryResponse
    {
        public IEnumerable<Transaction> Transactions { get; set; }
        public int TotalItemsCount { get; set; }
        public int PageSize { get; set; }
    }
}
