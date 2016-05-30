using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels
{
    public class TemporarySelfExclusionModel : PlayerMessageTemplateModel
    {
        public DateTimeOffset SelfExclusionExpiry { get; set; }
    }
}
