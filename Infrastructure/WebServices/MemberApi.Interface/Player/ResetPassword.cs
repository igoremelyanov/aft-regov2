using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ResetPasswordRequest 
    {
        /// <summary>
        /// Username or email
        /// </summary>
        public string Id { get; set; }
    }

    public class ResetPasswordResponse
    {
    }

    public class PlayerByResetPasswordTokenRequest
    {
        public string Token { get; set; }
    }

    public class PlayerByResetPasswordTokenResponse
    {
        public Guid PlayerId { get; set; }
    }
}
