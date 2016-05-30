using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class PlayerRegistrationChecked : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public SystemAction Action { get; set; }
        public QueueFolderTag Tag { get; set; }
        public DateTimeOffset DateChecked { get; set; }
        public int Score { get; set; }
    }
}
