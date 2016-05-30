using System;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class BrandId
    {
        public Guid AdminId { get; set; }
        public Guid Id { get; set; }
        public virtual Admin Admin { get; set; }
    }
}
