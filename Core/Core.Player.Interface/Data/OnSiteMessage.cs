using System;

namespace AFT.RegoV2.Core.Player.Interface.Data
{
    public class OnSiteMessage
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Received { get; set; }
        public bool IsNew { get; set; }
    }
}