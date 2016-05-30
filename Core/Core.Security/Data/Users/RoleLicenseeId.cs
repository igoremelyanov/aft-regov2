using System;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class RoleLicenseeId
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }

        public Role Role { get; set; }
    }
}
