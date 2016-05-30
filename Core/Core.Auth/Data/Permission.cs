using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Auth.Data
{
    public class Permission
    {
        public Permission()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Module { get; set; }

        public List<Role> Roles { get; set; }
    }
}
