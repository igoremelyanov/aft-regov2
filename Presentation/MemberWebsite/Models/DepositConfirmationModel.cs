namespace AFT.RegoV2.MemberWebsite.Models
{
    public class DepositConfirmationModel
    {
        public decimal DepositAmount { get; set; }
        public string DepositAmountFormatted { get; set; }
        public string Bonus { get; set; }
        public decimal Total { get; set; }
        public string TotalFormatted { get; set; }
        public decimal BonusAmount { get; set; }
        public string BonusAmountFormatted { get; set; }
        public string CurrencyCode { get; set; }
    }
}