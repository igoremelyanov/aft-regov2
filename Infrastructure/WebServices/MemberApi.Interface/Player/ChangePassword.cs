namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ChangePasswordRequest 
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }

    public class ChangePasswordResponse
    {
        public string UriToUserWithUpdatedPassword { get; set; }
    }
}
