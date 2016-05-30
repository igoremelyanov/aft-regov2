using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class TimeOutModel : PlayerMessageTemplateModel
    {
        public DateTimeOffset TimeOutExpiry { get; set; }
    }
}
