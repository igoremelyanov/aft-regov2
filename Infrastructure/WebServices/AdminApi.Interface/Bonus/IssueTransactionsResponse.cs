using System;

namespace AFT.RegoV2.AdminApi.Interface.Bonus
{
    public class IssueTransactionsResponse
    {
        public IssueTransaction[] Transactions;
    }

    public class IssueTransaction
    {
        public Guid Id;
        public decimal Amount;
        public decimal BonusAmount;
        public string CurrencyCode;
        public string Date;
    }
}