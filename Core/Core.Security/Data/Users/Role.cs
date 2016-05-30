using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class Role
    {
        public Role()
        {
            Licensees = new List<RoleLicenseeId>();
        }

        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<RoleLicenseeId> Licensees { get; set; }

        public Admin CreatedBy { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public Admin UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }
    }
}
