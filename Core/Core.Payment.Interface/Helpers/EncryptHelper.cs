using System.Security.Cryptography;
using System.Text;

namespace AFT.RegoV2.Core.Payment.Interface.Helpers
{
    public class EncryptHelper
    {
        public static string GetMD5HashInHexadecimalFormat(string plainText)
        {
            var md5 = MD5.Create();
            var result = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            var stringBuilder = new StringBuilder();
            foreach (var b in result)
            {
                stringBuilder.AppendFormat("{0:X2}", b);
            }
            return stringBuilder.ToString();
        }
    }
}
