namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public abstract class BrandMessageTemplateModel : IBrandMessageTemplateModel
    {
        public string BrandName { get; set; }
        public string RecipientName { get; set; }
        public string BrandWebsite { get; set; }
    }
}