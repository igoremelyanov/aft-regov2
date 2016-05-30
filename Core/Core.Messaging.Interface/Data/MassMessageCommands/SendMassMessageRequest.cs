using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands
{
    public class SendMassMessageRequest
    {
        public Guid Id { get; set; }
        public SendMassMessageContent[] Content { get; set; }
    }

    public class SendMassMessageContent
    {
        public string LanguageCode { get; set; }
        public bool OnSite { get; set; }
        public string OnSiteSubject { get; set; }
        public string OnSiteContent { get; set; }
    }
}