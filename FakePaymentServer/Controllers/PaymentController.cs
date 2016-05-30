using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using AFT.RegoV2.FakePaymentServer.Models;
using Nancy;
using Nancy.IO;
using Nancy.ModelBinding;

namespace AFT.RegoV2.FakePaymentServer.Controllers
{
    public class PaymentController : NancyModule
    {
        #region fileds
        private const string UserAgent = "Aft.Payment Notifier";
        private const string Acknowledge = "SUCCESS";
        private string OnlineDepositKey
        {
            get
            {
                var testKey = "testKey";
                if (ConfigurationManager.AppSettings["OnlineDepositKey"] != null)
                    testKey = ConfigurationManager.AppSettings["OnlineDepositKey"];
                return testKey;
            }
        }

        private string OnlineDepositMerchantId
        {
            get
            {
                var testKey = "TestMerchantId";
                if (ConfigurationManager.AppSettings["OnlineDepositMerchantId"] != null)
                    testKey = ConfigurationManager.AppSettings["OnlineDepositMerchantId"];
                return testKey;
            }
        }
        #endregion

        #region constructor
        public PaymentController()
        {
            Get["/"] = _ =>
            {
                return "REGO 2 FakePaymentServer";
                
            };

            Post["/payment/issue"] = _ =>
            {
                var request=this.Bind<PayFormData>();

                var msg = ValidateInRequest(request);
                if (msg.Length == 0)
                {
                    return View["PayForm", request];
                }
                else
                {
                    return msg;
                }
            };

            Post["/payment/notify/{notifyType}"] = parameters =>
            {
                var notifyType = parameters.notifyType;
                var request = this.Bind<PayFormData>();

                var response = GenerateNotification(request);

                if (notifyType == "1")
                {//return browser
                    var msg = NotifyBrand(response, request.NotifyUrl);

                    response.Action = request.ReturnUrl;
                    return View["RedirectToBrand", response];
                }
                else
                {//notify server
                    var msg = NotifyBrand(response,request.NotifyUrl);
                    request.Message = msg;
                    return View["PayForm", request];
                }                
            };      
        }
        #endregion 

        #region Methods
        private string ValidateInRequest(PayFormData request)
        {
            var msg = new StringBuilder();

            if (string.IsNullOrEmpty(request.Method))
            {
                msg.AppendFormat("{0} should not be null<BR>", "Method");
            }

            if (string.IsNullOrEmpty(request.MerchantId))
            {
                msg.AppendFormat("{0} should not be null<BR>", "MerchantId");
            }
            else
            {
                if (OnlineDepositMerchantId != request.MerchantId)
                {
                    msg.AppendFormat("{0}:{1} is wrong<BR>", "MerchantId", request.MerchantId);
                }
            }

            if (string.IsNullOrEmpty(request.Signature))
            {
                msg.AppendFormat("{0} should not be null<BR>", "Signature");
            }

            if (string.IsNullOrEmpty(request.OrderId))
            {
                msg.AppendFormat("{0} should not be null<BR>", "OrderId");
            }

            if (request.Amount.HasValue == false)
            {
                msg.AppendFormat("{0} should not be null<BR>", "Amount");
            }
            else
            {
                if (request.Amount.Value <= 0)
                {
                    msg.AppendFormat("{0} should be greater than 0<BR>", "Amount");
                }
            }

            if (request.Channel.HasValue == false)
            {
                msg.AppendFormat("{0} should not be null<BR>", "Channel");
            }

            if (string.IsNullOrEmpty(request.Currency))
            {
                msg.AppendFormat("{0} should not be null<BR>", "Currency");
            }

            if (string.IsNullOrEmpty(request.Language))
            {
                msg.AppendFormat("{0} should not be null<BR>", "Language");
            }

            if (string.IsNullOrEmpty(request.NotifyUrl))
            {
                msg.AppendFormat("{0} should not be null<BR>", "NotifyUrl");
            }

            if (string.IsNullOrEmpty(request.ReturnUrl))
            {
                msg.AppendFormat("{0} should not be null<BR>", "ReturnUrl");
            }

            if (msg.Length == 0)
            {         
                var signString = request.SignParams;
                var expect = GetMD5HashInHexadecimalFormat(signString + OnlineDepositKey);
                if (request.Signature != null && request.Signature != expect)
                {
                    msg.AppendFormat("{0} is not match,expect:{1},actual:{2}<BR>", "Signature", expect, request.Signature);
                }
            }

            return msg.ToString();
        }

        private string GetMD5HashInHexadecimalFormat(string plainText)
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

        private NotifactionData GenerateNotification(PayFormData request)
        {
            var orderId = DateTime.Now.ToString("yyyyMMddHHmmss");
            var response = new NotifactionData
            {
                Language = request.Language,
                OrderIdOfGateway = "GOID" + orderId,
                OrderIdOfMerchant = request.OrderId,
                OrderIdOfRouter = "ROID" + orderId,
                PayMethod = request.Method,                
            };
            response.Signature = GetMD5HashInHexadecimalFormat(response.SignParams + OnlineDepositKey);
            return response;
        }

        private string NotifyBrand(NotifactionData notification, string url)
        {
            var data = new NameValueCollection();
            data.Add("OrderIdOfGateway", notification.OrderIdOfGateway);
            data.Add("OrderIdOfMerchant", notification.OrderIdOfMerchant);
            data.Add("OrderIdOfRouter", notification.OrderIdOfRouter);
            data.Add("PayMethod", notification.PayMethod);            
            data.Add("Language", notification.Language);
            data.Add("Signature", notification.Signature);

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("user-agent", UserAgent);                
                try
                {
                    byte[] bytes = webClient.UploadValues(url, data);
                    var response = Encoding.UTF8.GetString(bytes);
                    var notified = Acknowledge.Equals(response, StringComparison.OrdinalIgnoreCase);
                    if(notified)
                        return String.Format("{0}", response);
                    else
                        return String.Format("Failed,response should be 'SUCCESS',but '{0}'", response);                    
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        #endregion
    }
}
