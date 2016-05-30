using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands
{
    public class UpdateRecipientsResponse
    {
        public Guid Id { get; set; }
        public bool HasRecipients { get; set; }
        public IEnumerable<Language> Languages { get; set; }
    }
}