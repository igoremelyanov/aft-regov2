namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class ContactVerificationModel : PlayerMessageTemplateModel
    {
        public string VerificationCode { get; set; }
        public string VerificationUrl { get; set; }
    }
}