using System;

namespace AFT.RegoV2.AdminWebsite
{
    [Serializable]
    public class AuthUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } // duplicating it here for consistency
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}