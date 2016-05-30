using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.GameProvider;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.Extensions;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Reports;
using AFT.RegoV2.MemberApi.Interface.Security;
using AFT.RegoV2.Shared.OAuth2;
using Newtonsoft.Json;
using OfflineDepositConfirm = AFT.RegoV2.MemberApi.Interface.Common.OfflineDepositConfirm;
using OfflineDepositRequest = AFT.RegoV2.MemberApi.Interface.Payment.OfflineDepositRequest;

namespace AFT.RegoV2.MemberApi.Interface.Proxy
{
    public class MemberApiProxy : OAuthProxy
    {
        public MemberApiProxy(string url, string token = null)
            : base(new Uri(url), token)
        {
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var result = await HttpClient.PostAsJsonAsync("api/Player/Register", request);
            return await EnsureApiResult<RegisterResponse>(result);
        }

        public async Task<ActivationResponse> ActivateAsync(string token)
        {
            var request = new ActivationRequest
            {
                Token = token
            };
            var result = await HttpClient.PostAsJsonAsync("api/Player/Activate", request);

            return await EnsureApiResult<ActivationResponse>(result);

        }

        private async Task<HttpResponseMessage> GetAccessTokenAsync(LoginRequest request)
        {
            var context = new Dictionary<string, string>
            {
                {"BrandId", request.BrandId.ToString()},
                {"IpAddress", request.IPAddress},
                {"BrowserHeaders", JsonConvert.SerializeObject(request.RequestHeaders)}
            };

            return await RequestResourceOwnerPasswordAsync("/token",
                request.Username, request.Password, additionalValues: context);
        }

        public async Task<TokenResponse> Login(LoginRequest request)
        {
            var result = await GetAccessTokenAsync(request);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                var content = await result.Content.ReadAsStringAsync();
                var details = await result.Content.ReadAsAsync<UnauthorizedDetails>();
                if (details?.error_description != null)
                {
                    var apiException = JsonConvert.DeserializeObject<MemberApiException>(details.error_description);

                    throw new MemberApiProxyException(apiException, result.StatusCode);
                }

                throw new MemberApiProxyException(new MemberApiException
                {
                    ErrorMessage = content
                }, result.StatusCode);
            }

            var tokenResponse = await result.Content.ReadAsAsync<TokenResponse>();
            Token = tokenResponse.AccessToken;

            return tokenResponse;
        }

        public async Task<LogoutResponse> Logout(LogoutRequest request)
        {
            var apiResult = await HttpClient.PostAsJsonAsync("api/Player/Logout", request);

            var result = await EnsureApiResult<LogoutResponse>(apiResult);

            Token = null;

            return result;
        }

        public async Task<ProfileResponse> ProfileAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/Profile");

            return await EnsureApiResult<ProfileResponse>(result);
        }

        public async Task<SecurityQuestionsResponse> SecurityQuestionsAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/SecurityQuestions");

            return await EnsureApiResult<SecurityQuestionsResponse>(result);

        }

        public async Task<RegistrationFormDataResponse> RegistrationFormDataAsync(RegistrationFormDataRequest request)
        {
            var query = "brandId=" + request.BrandId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/RegistrationFormData", query);

            return await EnsureApiResult<RegistrationFormDataResponse>(result);
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangePassword", request);

            return await EnsureApiResult<ChangePasswordResponse>(result);
        }

        public async Task<BalancesResponse> GetBalancesAsync(BalancesRequest request)
        {
            var query = "WalletId=" + request.WalletId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/Balances", query);
            return await EnsureApiResult<BalancesResponse>(result);
        }

        public async Task<BalanceSetResponse> GetBalancesSetAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/BalancesSet");
            return await EnsureApiResult<BalanceSetResponse>(result);
        }

        public async Task<WalletsDataResponse> GetWalletsAsync(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetWallets", query);
            return await EnsureApiResult<WalletsDataResponse>(result);
        }

        public async Task<OfflineDepositFormDataResponse> GetOfflineDepositFormDataAsync(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/OfflineDepositFormData", query);

            return await EnsureApiResult<OfflineDepositFormDataResponse>(result);
        }

        public async Task<BankAccountIdSettings> GetBankAccountForOfflineDeposit(Guid offlineDepositId)
        {
            var query = "offlineDepositId=" + offlineDepositId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetBankAccountForOfflineDeposit", query);
            return await EnsureApiResult<BankAccountIdSettings>(result);
        }

        public async Task<PaymentSettingsResponse> GetOnlinePaymentSetting(Guid brandId, string currencyCode)
        {
            var query = "brandId=" + brandId + "&currencyCode=" + currencyCode;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/OnlineDepositPaymentSettings", query);

            return await EnsureApiResult<PaymentSettingsResponse>(result);
        }

        public async Task<PaymentSettingsResponse> GetOfflinePaymentSetting(Guid brandId, string currencyCode)
        {
            var query = "brandId=" + brandId + "&currencyCode=" + currencyCode;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/OfflineDepositPaymentSettings", query);

            return await EnsureApiResult<PaymentSettingsResponse>(result);
        }

        public async Task<FundTransferFormDataResponse> GetFundTransferFormDataAsync(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result =
                await
                    HttpClient.SecureGetAsync(Token, "api/Payment/FundTransferFormData", query);

            return await EnsureApiResult<FundTransferFormDataResponse>(result);
        }

        public async Task<WithdrawalFormDataResponse> GetWithdrawalFormDataAsync(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result =
                await
                    HttpClient.SecureGetAsync(Token, "api/Payment/WithdrawalFormData", query);

            return await EnsureApiResult<WithdrawalFormDataResponse>(result);
        }

        public async Task<OfflineDepositResponse> OfflineDepositAsync(OfflineDepositRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/OfflineDeposit", request);

            return await EnsureApiResult<OfflineDepositResponse>(result);
        }

        public async Task<OfflineWithdrawalResponse> OfflineWithdrawAsync(WithdrawalRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/OfflineWithdraw", request);

            return await EnsureApiResult<OfflineWithdrawalResponse>(result);
        }

        public async Task<FundResponse> FundInAsync(FundRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/FundIn", request);

            return await EnsureApiResult<FundResponse>(result);
        }

        public async Task<ReferFriendsResponse> ReferFriendsAsync(ReferFriendsRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ReferFriends", request);

            return await EnsureApiResult<ReferFriendsResponse>(result);
        }

        public async Task<GameListResponse> GameListAsync(GamesRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Games/GameList", request);

            return await EnsureApiResult<GameListResponse>(result);
        }

        public async Task<GamesResponse> GamesAsync(GamesRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Games/GamesData", request);

            return await EnsureApiResult<GamesResponse>(result);
        }

        public async Task<ChangePersonalInfoResponse> ChangePersonalInfoAsync(ChangePersonalInfoRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangePersonalInfo", request);

            return await EnsureApiResult<ChangePersonalInfoResponse>(result);
        }

        public async Task<SelfExclusionResponse> SelfExclude(SelfExclusionRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/SelfExclude", new
            {
                request.PlayerId,
                request.Option
            });

            return await EnsureApiResult<SelfExclusionResponse>(result);
        }

        public async Task<TimeOutResponse> TimeOut(TimeOutRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/TimeOut", new
            {
                request.PlayerId,
                request.Option
            });

            return await EnsureApiResult<TimeOutResponse>(result);
        }

        public async Task<ChangeContactInfoResponse> ChangeContactInfoAsync(ChangeContactInfoRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangeContactInfo", request);

            return await EnsureApiResult<ChangeContactInfoResponse>(result);
        }

        public async Task<ChangeSecurityQuestionResponse> ChangeSecurityQuestionAsync(
            ChangeSecurityQuestionRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangeSecurityQuestion", request);

            return await EnsureApiResult<ChangeSecurityQuestionResponse>(result);
        }

        public async Task<VerificationCodeResponse> VerificationCodeAsync(VerificationCodeRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/VerificationCode", request);

            return await EnsureApiResult<VerificationCodeResponse>(result);
        }

        public async Task<VerifyMobileResponse> VerifyMobileAsync(VerifyMobileRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/VerifyMobile", request);

            return await EnsureApiResult<VerifyMobileResponse>(result);
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ResetPassword", request);

            return await EnsureApiResult<ResetPasswordResponse>(result);
        }

        public async Task<GetSecurityQuestionResponse> GetSecurityQuestionAsync(Guid playerId)
        {
            var query = "playerId=" + playerId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetSecurityQuestion", query);

            return await EnsureApiResult<GetSecurityQuestionResponse>(result);
        }

        public LanguagesResponse GetAvailableLanguages(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "api/Player/Languages", query)).Result;


            return EnsureApiResultSync<LanguagesResponse>(result);
        }

        public async Task<BonusRedemptionsResponse> BonusRedemptionsAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/BonusRedemptions");

            return await EnsureApiResult<BonusRedemptionsResponse>(result);
        }

        public async Task<IEnumerable<QualifiedBonus>> QualifiedFirstDepositBonuses(QualifiedBonusRequest request)
        {
            var query = "amout=" + request.Amount;
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/QualifiedFirstDepositBonuses", query);

            return await EnsureApiResult<IEnumerable<QualifiedBonus>>(result);
        }

        public async Task<IEnumerable<QualifiedBonus>> GetVisibleDepositQualifiedBonuses(QualifiedBonusRequest request)
        {
            var query = "amount=" + request.Amount;
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/GetVisibleDepositQualifiedBonuses", query);

            return await EnsureApiResult<IEnumerable<QualifiedBonus>>(result);
        }

        public async Task<IEnumerable<GameDTO>> GameDtos()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Games/GameDtos");

            return await EnsureApiResult<IEnumerable<GameDTO>>(result);
        }

        public async Task<QualifiedBonus> GetFirstDepositBonuseByCode(FirstDepositApplicationRequest request)
        {
            var query = string.Format("?playerId={0}&depositAmount={1}&bonusCode={2}", request.PlayerId, request.DepositAmount,
                request.BonusCode);
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/GetFirstDepositBonuseByCode", query);
            return await EnsureApiResult<QualifiedBonus>(result);
        }

        public async Task<TransactionListResponse> PlayerTransactionItems(int page, DateTime? startDate, DateTime? endDate, Guid? gameId)
        {
			var query = "page=" + page;

			if (startDate != null)
				query += "&startDate=" + startDate.Value;

			if (endDate != null)
				query += "&endDate=" + endDate.Value;

			if (gameId != null)
				query += "&gameId=" + gameId.Value;
			var result = await HttpClient.SecureGetAsync(Token, "api/Report/PlayerTransactionItems", query);
            return await EnsureApiResult<TransactionListResponse>(result);
        }

        public async Task<ClaimRedemptionResponse> ClaimRedemptionAsync(ClaimRedemptionRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Bonus/ClaimRedemption", request);

            return await EnsureApiResult<ClaimRedemptionResponse>(result);
        }

        // Security
        public VerifyIpResponse VerifyIp(VerifyIpRequest request)
        {
            // this call is used in an attribute. If I make this method async, the application hangs
            var result =
                Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "api/Security/VerifyIp", request)).Result;

            return EnsureApiResultSync<VerifyIpResponse>(result);
        }

        public async Task ApplicationErrorAsync(ApplicationErrorRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Security/LogApplicationError", request);

            await EnsureApiResult<ApplicationErrorResponse>(result);
        }

        public async Task<PendingDepositsResponse> GetPendingDeposits()
        {
            var result =
                await
                    HttpClient.SecureGetAsync(Token, "api/Payment/PendingDeposits");

            return await EnsureApiResult<PendingDepositsResponse>(result);
        }

        public async Task<PlayerData> GetPlayerData(string playerName)
        {
            var query = "playerName=" + playerName;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetPlayerData", query);

            return await EnsureApiResult<PlayerData>(result);
        }

        public async Task<PlayerByResetPasswordTokenResponse> GetPlayerByResetPasswordToken(string token)
        {
            var result = await HttpClient.PostAsJsonAsync("api/Player/GetPlayerByResetPasswordToken",
                new PlayerByResetPasswordTokenRequest
                {
                    Token = token
                });

            return await EnsureApiResult<PlayerByResetPasswordTokenResponse>(result);
        }

        public async Task<OfflineDeposit> GetOfflineDeposit(Guid id)
        {
            var query = "Id=" + id;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetOfflineDeposit", query);
            return await EnsureApiResult<OfflineDeposit>(result);
        }

        public async Task<ClaimableRedemption> GetRedemption(Guid redemptionId)
        {
            var query = "redemptionId=" + redemptionId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/GetRedemption", query);
            return await EnsureApiResult<ClaimableRedemption>(result);
        }

        public async Task<IEnumerable<QualifiedBonus>> GetQualifiedBonuses(QualifiedBonusRequest request)
        {
            var query = "amount=" + request.Amount;
            var result =
                await HttpClient.SecureGetAsync(Token, "api/Bonus/GetQualifiedBonuses", query);
            return await EnsureApiResult<IEnumerable<QualifiedBonus>>(result);
        }

        public async Task<OfflineDepositConfirmResponse> ConfirmOfflineDeposit(HttpRequest request)
        {
            var depositConfirm = request.Form["depositConfirm"];

            var confirm = JsonConvert.DeserializeObject<OfflineDepositConfirm>(depositConfirm);
            var uploadIdFront = request.Files["uploadId1"];
            var uploadIdBack = request.Files["uploadId2"];
            var receiptUpLoad = request.Files["receiptUpLoad"];

            confirm.IdFrontImage = uploadIdFront?.FileName;
            confirm.IdBackImage = uploadIdBack?.FileName;
            confirm.ReceiptImage = receiptUpLoad?.FileName;

            var confirmRequest = new OfflineDepositConfirmRequest
            {
                DepositConfirm = confirm,
                IdFrontImage = uploadIdFront?.InputStream.ToByteArray(),
                IdBackImage = uploadIdBack?.InputStream.ToByteArray(),
                ReceiptImage = receiptUpLoad?.InputStream.ToByteArray()
            };

            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/ConfirmDeposit", confirmRequest);

            return await EnsureApiResult<OfflineDepositConfirmResponse>(result);
        }

        public async Task<IEnumerable<BankResponse>> GetBanks()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetBanks");

            return await EnsureApiResult<IEnumerable<BankResponse>>(result);
        }

        public async Task<bool> IsDepositorsFullNameValid(string name)
        {
            var query = "name=" + name;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/IsDepositorsFullNameValid", query);

            return await EnsureApiResult<bool>(result);
        }

        public async Task<OnlineDepositFormDataResponse> GetOnlineDepositFormDataAsync(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result =
                await
                    HttpClient.SecureGetAsync(Token, "api/Payment/OnlineDepositFormData", query);

            return await EnsureApiResult<OnlineDepositFormDataResponse>(result);
        }

        public async Task<OnlineDepositResponse> OnlineDepositAsync(OnlineDepositRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/SubmitOnlineDeposit", request);

            return await EnsureApiResult<OnlineDepositResponse>(result);
        }

        public async Task<string> OnlineDepositPayNotifyAsync(OnlineDepositPayNotifyRequest request)
        {
            var result = await HttpClient.PostAsJsonAsync("api/Payment/OnlineDepositPayNotify", request);

            return await EnsureApiResult<string>(result);
        }

        public async Task<CheckOnlineDepositStatusResponse> CheckOnlineDepositStatusAsync(CheckOnlineDepositStatusRequest request)
        {
            var query = "transactionNumber=" + request.TransactionNumber;
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/CheckOnlineDepositStatus", query);

            return await EnsureApiResult<CheckOnlineDepositStatusResponse>(result);
        }

        public async Task<AcknowledgementData> GetAcknowledgementDataAsync(Guid id)
        {
            var query = "id=" + id;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetAcknowledgementData", query);

            return await EnsureApiResult<AcknowledgementData>(result);
        }

        public async Task<ValidationResult> ValidateRegisterInfo(RegisterRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ValidateRegisterInfo", request);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<OnSiteMessageResponse> GetOnSiteMessageAsync(OnSiteMessageRequest request)
        {
            var query = "onSiteMessageId=" + request.OnSiteMessageId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetOnSiteMessage", query);

            return await EnsureApiResult<OnSiteMessageResponse>(result);
        }

        public async Task<ValidationResult> ValidateResetPasswordRequestAsync(ResetPasswordRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ValidateResetPasswordRequest", request);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<ValidationResult> ValidateSecurityAnswerRequestAsync(SecurityAnswerRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ValidateSecurityAnswerRequest", request);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<SecurityAnswerCheckResponse> ConfirmSecurityAnswerRequestAsync(SecurityAnswerRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ConfirmSecurityAnswer", request);

            return await EnsureApiResult<SecurityAnswerCheckResponse>(result);
        }

        public async Task<ValidationResult> ValidateConfirmResetPasswordRequestAsync(ConfirmResetPasswordRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ValidateConfirmResetPasswordRequest", request);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<ConfirmResetPasswordResponse> ConfirmConfirmResetPasswordRequestAsync(ConfirmResetPasswordRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ConfirmResetPassword", request);

            return await EnsureApiResult<ConfirmResetPasswordResponse>(result);
        }

        public async Task<OnSiteMessagesResponse> GetOnSiteMessagesAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetOnSiteMessages");

            return await EnsureApiResult<OnSiteMessagesResponse>(result);
        }
        public async Task<ValidationResult> ValidateOnlineDepositAmount(
            ValidateOnlineDepositAmount request)
        {
            var query = "brandId=" + request.BrandId + "&amount=" + request.Amount;
            var result =
                await HttpClient.SecureGetAsync(Token, "api/Payment/ValidateOnlineDepositAmount", query);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<OnSiteMessagesCountResponse> GetOnSiteMessagesCountAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/GetOnSiteMessagesCount");

            return await EnsureApiResult<OnSiteMessagesCountResponse>(result);
        }

        public async Task<FirstDepositApplicationResponse> ValidateFirstOnlineDeposit(
            FirstDepositApplicationRequest request)
        {
            var query = "&bonusCode=" + request.BonusCode + "&depositAmount=" + request.DepositAmount;
            var result =
                await HttpClient.SecureGetAsync(Token, "api/Bonus/ValidateFirstOnlineDeposit", query);

            return await EnsureApiResult<FirstDepositApplicationResponse>(result);
        }

        public async Task<PlayerLastDepositSummaryResponse> PlayerLastDepositSummaryResponse()
        {
            var result =
                await HttpClient.SecureGetAsync(Token, "api/Payment/PlayerLastDepositSummaryResponse");

            return await EnsureApiResult<PlayerLastDepositSummaryResponse>(result);

        }

        public async Task<IEnumerable<ActiveBonus>> GetBonusesWithIncompleteWagering()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/GetBonusesWithInCompleteWagering");

            return await EnsureApiResult<IEnumerable<ActiveBonus>>(result);
        }

        public async Task<IEnumerable<ActiveBonus>> GetCompleteBonuses(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = string.Empty;

            if (startDate != null)
                query += "&startDate=" + startDate.Value;

            if (endDate != null)
                query += "&endDate=" + endDate.Value;

            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/GetCompleteBonuses", query);

            return await EnsureApiResult<IEnumerable<ActiveBonus>>(result);
        }

        public async Task<ValidationResult> ValidatePlayerBankAccount(PlayerBankAccountRequest request)
        {
            var query = "playerId=" + request.PlayerId +
                        "&bank=" + request.Bank +
                        "&accountName=" + request.AccountName +
                        "&accountNumber=" + request.AccountNumber +
                        "&province=" + request.Province +
                        "&city=" + request.City +
                        "&branch=" + request.Branch;

            var result =
                await HttpClient.SecureGetAsync(Token, "api/Payment/ValidatePlayerBankAccount", query);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<PlayerBankAccountResponse> CreatePlayerBankAccount(PlayerBankAccountRequest request)
        {
            var result =
                await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/CreatePlayerBankAccount", request);

            return await EnsureApiResult<PlayerBankAccountResponse>(result);
        }

        public async Task<ValidationResult> ValidateWithdrawalRequest(WithdrawalRequest request)
        {
            var result =
                await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/ValidateWithdrawalRequest", request);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<ValidationResult> ValidateConfirmOfflineDepositRequest(OfflineDepositConfirm request)
        {
            var result =
                await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/ValidateConfirmDeposit", request);

            return await EnsureApiResult<ValidationResult>(result);
        }

        public async Task<OfflineDepositBankAccountResponse> GetBankAccountsForOfflineDeposit()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetBankAccountsForOfflineDeposit");

            return await EnsureApiResult<OfflineDepositBankAccountResponse>(result);
        }

        public async Task<bool> ArePlayersIdDocumentsValid()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/ArePlayersIdDocumentsValid");

            return await EnsureApiResult<bool>(result);
        }

        public async Task<DepositHistoryResponse> GetDeposits(int page, DateTime? startDate, DateTime? endDate, DepositType? depositType)
        {
            var query = "page=" + page;

            if (startDate != null)
                query += "&startDate=" + startDate.Value;

            if (endDate != null)
                query += "&endDate=" + endDate.Value;

            if (depositType != null)
                query += "&depositType=" + depositType.Value;

            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetDeposits", query);

            return await EnsureApiResult<DepositHistoryResponse>(result);
        }

        public async Task<TransactionHistoryResponse> GetTransactions(int page, DateTime? startDate, DateTime? endDate, TransactionType? transactionType)
        {
            var query = "page=" + page;

            if (startDate != null)
                query += "&startDate=" + startDate.Value;

            if (endDate != null)
                query += "&endDate=" + endDate.Value;

            if (transactionType != null)
                query += "&transactionType=" + transactionType.Value;

            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetTransactions", query);

            return await EnsureApiResult<TransactionHistoryResponse>(result);
        }

        public async Task<WithdrawalHistoryResponse> GetWithdrawals(int page, DateTime? startDate, DateTime? endDate)
        {
            var query = "page=" + page;

            if (startDate != null)
                query += "&startDate=" + startDate.Value;

            if (endDate != null)
                query += "&endDate=" + endDate.Value;

            var result = await HttpClient.SecureGetAsync(Token, "api/Payment/GetWithdrawals", query);

            return await EnsureApiResult<WithdrawalHistoryResponse>(result);
        }
    }
}


