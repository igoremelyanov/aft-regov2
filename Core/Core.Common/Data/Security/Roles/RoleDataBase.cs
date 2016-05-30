using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles
{
    public class RoleDataBase
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid LicenseeId { get; set; }

        public IList<Guid> AssignedLicensees { get; set; }

        public List<Guid> CheckedPermissions { get; set; }
    }
}
