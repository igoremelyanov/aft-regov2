using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class OnSiteMessageRequest 
    {
        public Guid OnSiteMessageId { get; set; }
    }

    public class OnSiteMessageResponse
    {
        public OnSiteMessage OnSiteMessage { get; set; }
    }

    public class OnSiteMessagesResponse
    {
        public IEnumerable<OnSiteMessage> OnSiteMessages { get; set; }
    }

    public class OnSiteMessagesCountResponse
    {
        public int Count { get; set; }
    }

    public class OnSiteMessage
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Received { get; set; }
        public bool IsNew { get; set; }
    }
}
