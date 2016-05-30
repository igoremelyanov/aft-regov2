using System;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class LicenseeId
    {
        public Guid Id { get; set; }
        public Guid AdminId { get; set; }

        public Admin Admin { get; set; }
    }
}
