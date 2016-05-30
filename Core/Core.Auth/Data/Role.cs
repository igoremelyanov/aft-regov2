using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Auth.Data
{
    public class Role
    {
        public Role()
        {
            Id = Guid.NewGuid();
            Permissions = new List<Permission>();
        }

        public Guid Id { get; set; }
        public virtual List<Permission> Permissions { get; set; }
    }
}
