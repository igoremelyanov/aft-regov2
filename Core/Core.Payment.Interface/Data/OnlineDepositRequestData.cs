using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class OnlineDepositRequest
    {
        [Required]
        public Guid PlayerId { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }

        public Guid? BonusId { get; set; }

        public string RequestedBy { get; set; }

        public string BonusCode { get; set; }

        public string CultureCode { get; set; }

        public string ReturnUrl { get; set; }

        public string NotifyUrl { get; set; }
    }

    public class SubmitOnlineDepositRequestResult
    {
        public Guid DepositId;

        public Uri RedirectUrl;

        public OnlineDepositParams RedirectParams { get; set; }
    }

    public class OnlineDepositParams
    {
        public string Method { get; set; }

        public int Channel { get; set; }

        public string MerchantId { get; set; }

        public string OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string Language { get; set; }

        public string ReturnUrl { get; set; }

        public string NotifyUrl { get; set; }

        public string UserIp { get; set; }

        public string BankId { get; set; }

        public string Signature { get; set; }

        public string SignParams
        {
            get
            {
                var plainText = Method + Channel.ToString("D", CultureInfo.InvariantCulture) + MerchantId + OrderId +
                                Amount.ToString("0.00", CultureInfo.InvariantCulture) + Currency + Language + ReturnUrl +
                                NotifyUrl;
                return plainText;
            }
        }
    }
}