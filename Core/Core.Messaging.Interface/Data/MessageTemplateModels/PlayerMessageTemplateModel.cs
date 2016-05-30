namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public abstract class PlayerMessageTemplateModel : IPlayerMessageTemplateModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BrandName { get; set; }
        public string BrandWebsite { get; set; }
    }
}