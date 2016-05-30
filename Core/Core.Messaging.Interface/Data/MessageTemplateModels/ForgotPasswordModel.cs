using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class ForgotPasswordModel : PlayerMessageTemplateModel
    {
        public string ResetPasswordUrl { get; set; }
        public DateTimeOffset ResetPasswordUrlExpiry { get; set; }
    }
}