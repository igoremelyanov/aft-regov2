namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class BonusWageringRequirementFormattedModel : PlayerMessageTemplateModel
    {
        public string RequiredWagerAmount { get; set; }
        public string BonusAmount { get; set; }
        public bool IsAfterWager { get; set; }
    }

    public class BonusWageringRequirementModel : PlayerMessageTemplateModel
    {
        public decimal RequiredWagerAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public bool IsAfterWager { get; set; }
    }
}