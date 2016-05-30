using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data.Security.Users
{
    public class AdminDataBase
    {
        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Language { get; set; }

        public bool IsActive { get; set; }

        public string Password { get; set; }

        public Guid? RoleId { get; set; }

        public string Description { get; set; }

        public IList<Guid> AssignedLicensees { get; set; }

        public IList<Guid> AllowedBrands { get; set; }

        public IList<string> Currencies { get; set; }
    }
}
