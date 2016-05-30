using System;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class Identity
    {
        public Identity()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}