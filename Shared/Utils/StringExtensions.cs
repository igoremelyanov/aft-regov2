using System.Security;
using System.Text.RegularExpressions;

namespace AFT.RegoV2.Shared.Utils
{
    public static class StringExtensions
    {
        public static string SeparateWords(this string combinedString)
        {
            return Regex.Replace(combinedString, "([A-Z])", " $1").Trim();
        }

        public static SecureString ConvertToSecureString(this string sourceString)
        {
            var result = new SecureString();
            if (!string.IsNullOrEmpty(sourceString))
            {
                foreach (var c in sourceString.ToCharArray()) result.AppendChar(c);
            }
            return result;
        }

        public static string Args(this string str, params object[] args)
        {
            return string.Format(str, args);
        }
    }
}
