using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class OnlineDepositFormDataRequest 
    {
        public Guid BrandId { get; set; }
    }

    public class OnlineDepositFormDataResponse
    {
        public IEnumerable<PaymentGatewaySettings> PaymentGatewaySettings { get; set; }
    }
    

    public class OnlineDepositRequest 
    {
        public Guid BrandId { get; set; }
        public decimal Amount { get; set; }
        public string BonusCode { get; set; }
        public Guid? BonusId { get; set; }
        public string CultureCode { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
    }

    public class OnlineDepositResponse
    {
        public SubmitOnlineDepositRequestResult DepositRequestResult { get; set; }
    }

    public class SubmitOnlineDepositRequestResult
    {
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

        public string SignParams { get; set; }
    }

    public class OnlineDepositPayNotifyRequest
    {
        public string OrderIdOfMerchant { get; set; }

        public string OrderIdOfRouter { get; set; }

        public string OrderIdOfGateway { get; set; }

        public string Language { get; set; }

        public string PayMethod { get; set; }

        public string Signature { get; set; }
    }

    public class CheckOnlineDepositStatusRequest
    {
        public string TransactionNumber { get; set; }        
    }

    public class CheckOnlineDepositStatusResponse
    {
        public CheckStatusResponse DepositStatus { get; set; }
    }
    public class CheckStatusResponse
    {
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
        public decimal Bonus { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class PaymentGatewaySettings
    {
        public Guid Id { get; set; }
        public string PaymentGatewayName { get; set; }
        public int Channel { get; set; }
    }
}
