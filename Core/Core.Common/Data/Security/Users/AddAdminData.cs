using System;

namespace AFT.RegoV2.Core.Common.Data.Security.Users
{
    public class AddAdminData : AdminDataBase
    {
        public Guid Id { get; set; }

        public string PasswordConfirmation { get; set; }

        public string RoleName { get; set; }
    }
}
