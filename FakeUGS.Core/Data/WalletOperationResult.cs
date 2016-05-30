namespace FakeUGS.Core.Data
{
    public class WalletOperationResult
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string ExternalTransactionId { get; set; }
        public string PlatformTransactionId { get; set; }
        public bool IsDuplicate { get; set; }
    }
}
