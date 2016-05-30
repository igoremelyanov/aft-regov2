namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class BonusWageringRequirementFulfilledFormattedModel : PlayerMessageTemplateModel
    {
        public string RequiredWagerAmount { get; set; }
        public string BonusAmount { get; set; }
    }

    public class BonusWageringRequirementFulfilledModel : PlayerMessageTemplateModel
    {
        public decimal RequiredWagerAmount { get; set; }
        public decimal BonusAmount { get; set; }
    }
}