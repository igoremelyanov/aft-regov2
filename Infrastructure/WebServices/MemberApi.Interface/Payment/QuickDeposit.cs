namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    
    public class QuickDepositRequest
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class QuickDepositResponse
    {
    }

}
