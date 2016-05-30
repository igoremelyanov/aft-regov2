using System;

namespace AFT.RegoV2.Shared.Constants
{
    public static class RoleIds
    {
        public static Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static Guid DefaultId = new Guid("00000000-0000-0000-0000-000000000002");
        public static Guid SystemId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        public static Guid MultipleBrandManagerId = Guid.Parse("00000000-0000-0000-0000-000000000009");
        public static Guid SingleBrandManagerId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        public static Guid LicenseeId = Guid.Parse("00000000-0000-0000-0000-000000000008");
    }
}