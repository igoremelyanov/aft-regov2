﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.AdminApi.Interface.Extensions;

namespace AFT.RegoV2.AdminApi.Filters
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
}