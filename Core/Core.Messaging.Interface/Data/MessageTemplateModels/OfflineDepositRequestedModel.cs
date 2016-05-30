namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class OfflineDepositRequestedModel : PlayerMessageTemplateModel
    {
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string[] RecommendedBanks { get; set; }
    }
}