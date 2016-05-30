namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class VerifyMobileRequest 
    {
        public string VerificationCode { get; set; }
    }

    public class VerifyMobileResponse 
    {
        public string UriToPlayerWhoseMobileWasVerified { get; set; }
    }
}