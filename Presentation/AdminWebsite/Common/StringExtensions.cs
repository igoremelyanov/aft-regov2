using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public static class StringExtensions
    {
        const int ShowMobileNumbers = 4;

        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;

            var pos = email.IndexOf('@');

            return email.Replace(email.Substring(0, pos), new string('*', pos));
        }

        public static string MaskMobile(this string mobile)
        {
            if (string.IsNullOrEmpty(mobile)) return string.Empty;

            var len = mobile.Length;

            return len <= ShowMobileNumbers ? new string('*', len) 
                : mobile.Replace(mobile.Substring(ShowMobileNumbers, len - ShowMobileNumbers), new string('*', len - ShowMobileNumbers));
        }
    }
}