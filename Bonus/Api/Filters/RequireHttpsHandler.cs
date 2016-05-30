using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AFT.RegoV2.Bonus.Api.Filters
{
    public class RequireHttpsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // if request is local, just serve it without https
            if (request.IsLocalRequest())
            {
                return base.SendAsync(request, cancellationToken);
            }

            // if request is remote, enforce https
            if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                return Task<HttpResponseMessage>.Factory.StartNew(
                    () =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                        {
                            Content = new StringContent("HTTPS Required")
                        };

                        return response;
                    });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

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