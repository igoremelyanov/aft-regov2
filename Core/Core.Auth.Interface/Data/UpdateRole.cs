using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Auth.Interface.Data
{
    public class UpdateRole
    {
        public Guid RoleId;
        public List<Guid> Permissions;
    }
}