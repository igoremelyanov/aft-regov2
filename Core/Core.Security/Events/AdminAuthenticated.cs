using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Security.Events
{
    public class AdminAuthenticationSucceded : DomainEventBase
    {
        public string IPAddress { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class AdminAuthenticationFailed : DomainEventBase
    {
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string FailReason { get; set; }
    }
}
