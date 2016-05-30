using System;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class CurrencyCode
    {
        public Guid AdminId { get; set; }

        public string Currency { get; set; }

        public Admin Admin { get; set; }
    }
}
