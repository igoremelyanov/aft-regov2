using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class SecurityViewModel
    {
        public string UserName { get; set; }

        public IEnumerable<PermissionViewModel> Operations { get; set; }

        public IEnumerable<Guid> UserPermissions { get; set; }

        public IEnumerable<Guid> Licensees { get; set; }

        public bool IsSingleBrand { get; set; }

        public bool IsSuperAdmin { get; set; }

        public IDictionary<string, string> Permissions { get; set; }

        public IDictionary<string, string> Categories { get; set; }
    }

    public class PermissionViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Module { get; set; }
    }
}