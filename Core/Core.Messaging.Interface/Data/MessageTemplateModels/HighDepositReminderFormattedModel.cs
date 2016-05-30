namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class HighDepositReminderFormattedModel : PlayerMessageTemplateModel
    {
        public string Currency { get; set; }
        public string BonusAmount { get; set; }
        public string RemainingAmount { get; set; }
        public string DepositAmountRequired { get; set; }
    }

    public class HighDepositReminderModel : PlayerMessageTemplateModel
    {
        public string Currency { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal DepositAmountRequired { get; set; }
    }
}