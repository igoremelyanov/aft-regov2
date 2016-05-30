using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberWebsite.Common;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Shared.OAuth2;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberWebsite.Controllers
{
    public class MemberApiController : ApiController
    {
        private readonly Guid _brandId;
        private readonly MemberApiProxy _memberApiProxy;

        public MemberApiController()
        {
            var appSettings = new AppSettings();
            _brandId = appSettings.BrandId;

            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            string token = null;
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                try
                {
                    var formsTicket = FormsAuthentication.Decrypt(cookie.Value);
                    if (formsTicket != null)
                    {
                        token = formsTicket.UserData;
                    }
                }
                catch (CryptographicException)
                {

                }
            }
            _memberApiProxy = new MemberApiProxy(appSettings.MemberApiUrl.ToString(), token);
        }

        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            request.BrandId = _brandId;
            var httpRequest = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request;

            request.IPAddress = GetIpRequest();
            request.RequestHeaders = httpRequest.Headers.ToDictionary();

            TokenResponse result;
            result = await _memberApiProxy.Login(request);

            var cookie = CreateAuthenticationCookie(result.AccessToken, request.Username, false);
            HttpContext.Current.Response.Cookies.Add(cookie);
            return Ok(new { Success = true });
        }

        public IHttpActionResult Logout(LogoutRequest request)
        {
            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                cookie.Expires = DateTime.Now.AddDays(-10);
                cookie.Value = null;
                HttpContext.Current.Response.SetCookie(cookie);
            }
            return Ok(new { });
        }

        public async Task<IHttpActionResult> Register(RegisterRequest request)
        {
            request.IpAddress = GetIpRequest();

            var settings = new AppSettings();
            request.AccountActivationEmailToken = Guid.NewGuid();
            request.AccountActivationEmailUrl = string.Format(
                "{0}{1}/Home/Activate?token={2}",
                settings.MemberWebsiteUrl,
                request.CultureCode,
                request.AccountActivationEmailToken);

            await _memberApiProxy.RegisterAsync(request);
            return await Login(new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });
        }

        [HttpGet]
        public async Task<IHttpActionResult> Profile()
        {
            var profile = await _memberApiProxy.ProfileAsync();
            return Ok(new { Success = true, Profile = profile });
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetBalance()
        {
            try
            {
                var balance = await _memberApiProxy.GetBalancesAsync(new BalancesRequest());
                return Ok(new { Success = true, Balance = balance });
            }
            catch (MemberApiProxyException) 
            {
                return Ok(new { Success = false});
            }
        }

        [HttpGet]
        public async Task<SecurityQuestionsResponse> SecurityQuestions()
        {
            return await _memberApiProxy.SecurityQuestionsAsync();
        }

        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request)
        {
            return await _memberApiProxy.ChangePasswordAsync(request);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ConfirmDepositHistory()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

            var dataStream = filesReadToProvider.Contents
                .Single(o => o.Headers.ContentDisposition.Name == "\"data\"");

            var dataJson = await dataStream.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<SomeType>(dataJson);

            foreach (var stream in filesReadToProvider.Contents)
            {
                var fileBytes = await stream.ReadAsByteArrayAsync();
            }

            return new HttpResponseMessage(HttpStatusCode.OK);

            /*var root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names.
                foreach (var file in provider.FileData)
                {
                    var fileName = file.Headers.ContentDisposition.FileName;
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }*/
        }

        public async Task<ValidationResult> ValidateConfirmDepositRequest(OfflineDepositConfirm request)
        {
            return await _memberApiProxy.ValidateConfirmOfflineDepositRequest(request);
        }

        public class SomeType
        {
            public string FullName { get; set; }
            public string Amount { get; set; }
            public string Remark { get; set; }
        }

        public async Task<OfflineDepositResponse> OfflineDeposit(OfflineDepositRequest request)
        {
            return await _memberApiProxy.OfflineDepositAsync(request);
        }

        public async Task<OfflineWithdrawalResponse> OfflineWithdrawal(WithdrawalRequest request)
        {
            return await _memberApiProxy.OfflineWithdrawAsync(request);
        }

        public async Task<IEnumerable<BankResponse>> GetBanks()
        {
            return await _memberApiProxy.GetBanks();
        }

        public async Task<FundResponse> FundIn(FundRequest request)
        {
            return await _memberApiProxy.FundInAsync(request);
        }

        public async Task<BonusRedemptionsResponse> GetBonusRedemptions()
        {
            return await _memberApiProxy.BonusRedemptionsAsync();
        }

        [HttpPost]
        public async Task<ClaimRedemptionResponse> ClaimBonusReward(ClaimRedemptionRequest request)
        {
            return await _memberApiProxy.ClaimRedemptionAsync(request);
        }

        public async Task<ReferFriendsResponse> ReferFriends(ReferFriendsRequest request)
        {
            return await _memberApiProxy.ReferFriendsAsync(request);
        }


        public async Task<ChangePersonalInfoResponse> ChangePersonalInfo(ChangePersonalInfoRequest request)
        {
            return await _memberApiProxy.ChangePersonalInfoAsync(request);
        }

        public async Task<SelfExclusionResponse> SelfExclude(SelfExclusionRequest request)
        {
            return await _memberApiProxy.SelfExclude(request);
        }

        public async Task<TimeOutResponse> TimeOut(TimeOutRequest request)
        {
            return await _memberApiProxy.TimeOut(request);
        }

        [HttpPost]
        public async Task<dynamic> ConfirmOfflineDeposit()
        {
            var request = HttpContext.Current.Request;

            try
            {
                await _memberApiProxy.ConfirmOfflineDeposit(request);
            }
            catch (Exception exception)
            {
                return new
                {
                    Result = "failed",
                    Message = exception.Message
                };
            }

            return new
            {
                Result = "success"
            };
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetOfflineDeposit(Guid id)
        {
            var deposit = await _memberApiProxy.GetOfflineDeposit(id);
            var playerName = HttpContext.Current.User.Identity.Name;
            var playerData = await _memberApiProxy.GetPlayerData(playerName);

            return Ok(new
            {
                Deposit = deposit,
                Player = playerData
            });
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ValidationResult> ValidateOnlineDepositAmount(ValidateOnlineDepositAmount request)
        {
            request.BrandId = new AppSettings().BrandId;
            var response = await _memberApiProxy.ValidateOnlineDepositAmount(request);
            return response;
        }

        [System.Web.Mvc.HttpPost]
        public async Task<FirstDepositApplicationResponse> ValidateFirstDepositBonus(FirstDepositApplicationRequest request)
        {
            var response = await _memberApiProxy.ValidateFirstOnlineDeposit(request);
            return response;
        }

        [System.Web.Mvc.HttpPost]
        public async Task<IEnumerable<QualifiedBonus>> QualifiedBonuses(QualifiedBonusRequest request)
        {
            var response = await _memberApiProxy.GetQualifiedBonuses(request);
            return response;
        }

        [HttpGet]
        public async Task<IEnumerable<QualifiedBonus>> GetQualifiedBonuses(decimal amount)
        {
            var response = await _memberApiProxy.GetQualifiedBonuses(new QualifiedBonusRequest
            {
                Amount = amount
            });

            return response;
        }

        [HttpGet]
        public async Task<IEnumerable<QualifiedBonus>> GetQualifiedBonuses()
        {
            var response = await _memberApiProxy.GetQualifiedBonuses(new QualifiedBonusRequest
            {
                Amount = null
            });

            return response;
        }

        [HttpGet]
        public async Task<IEnumerable<QualifiedBonus>> GetVisibleDepositQualifiedBonuses()
        {
            var response = await _memberApiProxy.GetVisibleDepositQualifiedBonuses(new QualifiedBonusRequest
            {
                Amount = null
            });

            return response;
        }

        public async Task<ChangeSecurityQuestionResponse> ChangeSecurityQuestion(ChangeSecurityQuestionRequest request)
        {
            return await _memberApiProxy.ChangeSecurityQuestionAsync(request);
        }

        public async Task<VerificationCodeResponse> VerificationCode(VerificationCodeRequest request)
        {
            return await _memberApiProxy.VerificationCodeAsync(request);
        }

        public async Task<VerifyMobileResponse> VerifyMobile(VerifyMobileRequest request)
        {
            return await _memberApiProxy.VerifyMobileAsync(request);
        }

        public async Task<ValidationResult> ValidatePlayerBankAccount(PlayerBankAccountRequest request)
        {
            return await _memberApiProxy.ValidatePlayerBankAccount(request);
        }

        public async Task<ValidationResult> ValidateWithdrawalRequest(WithdrawalRequest request)
        {
            return await _memberApiProxy.ValidateWithdrawalRequest(request);
        }

        public async Task<PlayerBankAccountResponse> CreatePlayerBankAccount(PlayerBankAccountRequest request)
        {
            return await _memberApiProxy.CreatePlayerBankAccount(request);
        }

        #region First ForgetPassword step

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
        {
            return await _memberApiProxy.ResetPasswordAsync(request);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ValidationResult> ValidateResetPasswordRequest(ResetPasswordRequest request)
        {
            return await _memberApiProxy.ValidateResetPasswordRequestAsync(request);
        }

        #endregion

        #region Second SecurityQuestion step

        public async Task<GetSecurityQuestionResponse> GetSecurityQuestion(Guid playerId)
        {
            return await _memberApiProxy.GetSecurityQuestionAsync(playerId);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ValidationResult> ValidateSecurityAnswerRequest(SecurityAnswerRequest request)
        {
            return await _memberApiProxy.ValidateSecurityAnswerRequestAsync(request);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<SecurityAnswerCheckResponse> SecurityAnswerRequest(SecurityAnswerRequest request)
        {
            return await _memberApiProxy.ConfirmSecurityAnswerRequestAsync(request);
        }

        #endregion

        #region Third ConfirmResetPassword step

        [HttpPost]
        [AllowAnonymous]
        public async Task<ValidationResult> ValidateConfirmResetPasswordRequest(ConfirmResetPasswordRequest request)
        {
            return await _memberApiProxy.ValidateConfirmResetPasswordRequestAsync(request);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ConfirmResetPasswordResponse> ConfirmResetPasswordRequest(ConfirmResetPasswordRequest request)
        {
            return await _memberApiProxy.ConfirmConfirmResetPasswordRequestAsync(request);
        }

        #endregion

        private static HttpCookie CreateAuthenticationCookie(string oauthToken, string userName, bool rememberMe)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                userName,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                rememberMe,
                oauthToken);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            return new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
        }

        public async Task<OnlineDepositResponse> OnlineDeposit(OnlineDepositRequest request)
        {
            var settings = new AppSettings();
            request.BrandId = _brandId;
            request.CultureCode = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            request.NotifyUrl = settings.MemberWebsiteUrl + request.CultureCode + "/Home/OnlineDepositPayNotify";
            request.ReturnUrl = settings.MemberWebsiteUrl + request.CultureCode + "/Home/OnlineDepositPayReceive";
            return await _memberApiProxy.OnlineDepositAsync(request);
        }

        public async Task<CheckOnlineDepositStatusResponse> CheckOnlineDepositStatus(
            CheckOnlineDepositStatusRequest request)
        {
            return await _memberApiProxy.CheckOnlineDepositStatusAsync(request);
        }

        public async Task<ValidationResult> ValidateRegisterInfo(RegisterRequest request)
        {
            return await _memberApiProxy.ValidateRegisterInfo(request);
        }

        [HttpGet]
        public async Task<OnSiteMessageResponse> GetOnSiteMessage([FromUri]OnSiteMessageRequest request)
        {
            return await _memberApiProxy.GetOnSiteMessageAsync(request);
        }

        [HttpGet]
        public async Task<OnSiteMessagesResponse> GetOnSiteMessages()
        {
            return await _memberApiProxy.GetOnSiteMessagesAsync();
        }

        [HttpGet]
        public async Task<BankAccountIdSettings> GetBankAccountForOfflineDeposit(Guid offlineDepositId)
        {
            return await _memberApiProxy.GetBankAccountForOfflineDeposit(offlineDepositId);
        }

        [HttpGet]
        public async Task<bool> IsDepositorsFullNameValid(string name)
        {
            return await _memberApiProxy.IsDepositorsFullNameValid(name);
        }

        [HttpGet]
        public async Task<bool> ArePlayersIdDocumentsValid()
        {
            return await _memberApiProxy.ArePlayersIdDocumentsValid();
        }

        [NonAction]
        private string GetIpRequest()
        {
            const string IPAddressServerVariableName = "REMOTE_ADDR";
            var httpRequest = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request;
            return httpRequest.ServerVariables[IPAddressServerVariableName];
        }
    }
}