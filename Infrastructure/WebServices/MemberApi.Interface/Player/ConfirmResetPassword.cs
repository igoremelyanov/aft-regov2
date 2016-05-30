using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ConfirmResetPasswordRequest
    {
        public Guid PlayerId { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class ConfirmResetPasswordResponse
    {
        
    }
}
