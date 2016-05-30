namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class RegistrationVerificationModel : PlayerMessageTemplateModel
    {
        public string VerificationCode { get; set; }
        public string VerificationUrl { get; set; }
    }
}