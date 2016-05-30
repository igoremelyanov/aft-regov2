using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

using AFT.UGS.Core.Messages.Lobby;

using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Extensions
{
    public static class HttpActionContextExtensions
    {
        public static Guid GetIdFromAuthToken(this HttpActionContext context, ITokenProvider tokenProvider)
        {
            IEnumerable<string> values;
            if (context.Request.Headers.TryGetValues("Authorization", out values))
            {
                var authorization = values.FirstOrDefault();
                if (authorization != null && authorization.StartsWith("Bearer", true, null))
                {
                    var token = authorization.Substring(6).Trim();
                    return tokenProvider.Decrypt(token);
                }
            }
            else
            {
                var req = context.ActionArguments.Values.OfType<IStringTokenHolder>().FirstOrDefault();
                if (req != null)
                {
                    return tokenProvider.Decrypt(req.token);
                }
            }

            throw new InvalidTokenException("Missing Authorization Bearer Header");
        }
    }
}