using System.Net;

namespace AFT.RegoV2.AdminApi.Interface.Proxy
{
    public class ProxyBase 
    {
        public WebClient WebClient { get; protected set; }

        protected ProxyBase()
        {}

        public void Dispose()
        {
            WebClient.Dispose();
        }
    }
}
