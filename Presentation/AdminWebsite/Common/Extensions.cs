using System;
using System.Web;
using System.Web.Security;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public static class Extensions
    {
        public static string AccessToken(this HttpRequestBase request)
        {
            var cookie = request.Cookies[FormsAuthentication.FormsCookieName];
            string token = null;
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                var formsTicket = FormsAuthentication.Decrypt(cookie.Value);
                if (formsTicket != null)
                {
                    token = formsTicket.UserData;
                }
            }
            return token;
        }
    }
}