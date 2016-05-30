using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Messaging.Interface.Data
{
    public class MassMessage
    {
        public MassMessage()
        {
            Recipients = new List<Player>();
            Content = new List<MassMessageContent>();
        }

        public Guid Id { get; set; }
        public Guid AdminId { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset? DateSent { get; set; }
        public ICollection<Player> Recipients { get; set; }
        public ICollection<MassMessageContent> Content { get; set; }
    }
}