using System.Net;
using System.Net.Http;

namespace AFT.RegoV2.AdminApi.Interface.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        private const string OwinContext = "MS_OwinContext";

        public static bool IsLocalRequest(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(OwinContext))
            {
                dynamic owinContext = request.Properties[OwinContext];
                
                if (owinContext != null)
                {
                    var remoteIpAddress = IPAddress.Parse(owinContext.Request.RemoteIpAddress);
                    return IPAddress.IsLoopback(remoteIpAddress);
                }
            }

            return false;
        }
    }
}