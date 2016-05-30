using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class OnSiteMessage
    {
        public Guid Id { get; set; }
        public Player Player { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Received { get; set; }
        public bool IsNew { get; set; }
    }
}