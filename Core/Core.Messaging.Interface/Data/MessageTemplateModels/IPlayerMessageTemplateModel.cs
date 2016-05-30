namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public interface IPlayerMessageTemplateModel : IMessageTemplateModel
    {
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string BrandName { get; set; }
        string BrandWebsite { get; set; }
    }
}