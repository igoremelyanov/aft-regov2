namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public interface IBrandMessageTemplateModel : IMessageTemplateModel
    {
        string BrandName { get; set; }
        string RecipientName { get; set; }
        string BrandWebsite { get; set; }
    }
}