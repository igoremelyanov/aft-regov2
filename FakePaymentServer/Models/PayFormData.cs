using System;
using System.Collections.Generic;
using System.Globalization;
namespace AFT.RegoV2.FakePaymentServer.Models
{
    public class PayFormData
    {
        public string Method { get; set; }
        public int? Channel { get; set; }
        public string MerchantId { get; set; }
        public string OrderId { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string Language { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string UserIp { get; set; }
        public string BankId { get; set; }
        public string Signature { get; set; }

        public string Message { get; set; }

        public string SignParams
        {
            get
            {
                var plainText = Method + Channel.Value.ToString("D", CultureInfo.InvariantCulture) + MerchantId + OrderId +
                                Amount.Value.ToString("0.00", CultureInfo.InvariantCulture) + Currency + Language + ReturnUrl +
                                NotifyUrl;
                return plainText;
            }
        }
    }
}
