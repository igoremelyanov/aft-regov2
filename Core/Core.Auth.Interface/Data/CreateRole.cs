using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Auth.Interface.Data
{
    public class CreateRole
    {
        public Guid RoleId;
        public List<Guid> Permissions;
    }
}
