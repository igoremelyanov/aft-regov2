// ReSharper disable InconsistentNaming
namespace AFT.RegoV2.GameApi.Interface.ServiceContracts.OAuth
{
    public class OAuth2Token
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
        public string scope { get; set; }
    }
}
