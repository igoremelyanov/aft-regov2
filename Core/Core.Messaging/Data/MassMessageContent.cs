using System;
using AFT.RegoV2.Core.Messaging.Interface.Data;

namespace AFT.RegoV2.Core.Messaging.Data
{
    public class MassMessageContent
    {
        public Guid Id { get; set; }
        public MassMessage MassMessage { get; set; }
        public MassMessageType MassMessageType { get; set; }
        public Language Language { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}