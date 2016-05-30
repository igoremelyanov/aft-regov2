namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class Transaction
    {
        public string CreatedOn { get; set; }
        public string TransactionType { get; set; }
        public string TransactionNumber { get; set; }
        public decimal Amount { get; set; }
        public string AmountFormatted { get; set; }
        public decimal MainBalance { get; set; }
        public string MainBalanceFormatted { get; set; }
    }
}
