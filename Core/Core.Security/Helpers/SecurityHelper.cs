using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Core.Security.Helpers
{
    public static class RoleExtensions
    {
        public static void SetLicensees(this Role role, IEnumerable<Guid> licensees)
        {
            if (licensees == null) 
                return;
            role.Licensees.Clear();

            foreach (var licensee in licensees)
            {
                var licenseeId = new RoleLicenseeId
                {
                    Id = licensee,
                    RoleId = role.Id
                };

                role.Licensees.Add(licenseeId);
            }
        }
    }
}
