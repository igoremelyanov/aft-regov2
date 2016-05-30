namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class CheckStatusRequest
    {
        public string TransactionNumber { get; set; }
    }

    public class CheckStatusResponse
    {
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
        public decimal Bonus { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
