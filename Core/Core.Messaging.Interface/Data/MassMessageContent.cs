using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Messaging.Interface.Data
{
    public class MassMessageContent
    {
        public Guid Id { get; set; }
        public MassMessageType MassMessageType { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}