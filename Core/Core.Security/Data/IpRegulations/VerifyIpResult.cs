namespace AFT.RegoV2.Core.Security.Data.IpRegulations
{
    public class VerifyIpResult
    {
        public bool Allowed { get; set; }
        public string BlockingType { get; set; }
        public string RedirectionUrl { get; set; }
    }
}
