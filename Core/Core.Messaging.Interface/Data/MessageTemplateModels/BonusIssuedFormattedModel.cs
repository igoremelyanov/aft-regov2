namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class BonusIssuedFormattedModel : PlayerMessageTemplateModel
    {
        public string Amount { get; set; }
    }

    public class BonusIssuedModel : PlayerMessageTemplateModel
    {
        public decimal Amount { get; set; }
    }
}