using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AFT.RegoV2.AdminApi.Interface.Bonus;
using AFT.RegoV2.AdminApi.Interface.Brand;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Extensions;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminApi.Interface.Player;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Shared.OAuth2;
using Newtonsoft.Json;
using BonusRedemptionResponse = AFT.RegoV2.AdminApi.Interface.Bonus.BonusRedemptionResponse;
using TemplateDataResponse = AFT.RegoV2.AdminApi.Interface.Bonus.TemplateDataResponse;

namespace AFT.RegoV2.AdminApi.Interface.Proxy
{
    public class AdminApiProxy : OAuthProxy
    {
        private readonly string _url;

        public AdminApiProxy(string url, string token = null) : base(new Uri(url), token)
        {
            _url = url;
        }

        private async Task<HttpResponseMessage> GetAccessTokenAsync(LoginRequest request)
        {
            return await RequestResourceOwnerPasswordAsync("/token",
                request.Username, request.Password);
        }

        public async Task<TokenResponse> Login(LoginRequest request)
        {
            var result = await GetAccessTokenAsync(request);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                var content = await result.Content.ReadAsStringAsync();
                var details = await result.Content.ReadAsAsync<UnauthorizedDetails>();
                if (details != null && details.error_description != null)
                {
                    var apiException = JsonConvert.DeserializeObject<AdminApiException>(details.error_description);
                    throw new AdminApiProxyException(apiException, result.StatusCode);
                }

                throw new AdminApiProxyException(new AdminApiException
                {
                    ErrorMessage = content
                }, result.StatusCode);
            }

            var tokenResponse = await result.Content.ReadAsAsync<TokenResponse>();

            Token = tokenResponse.AccessToken;

            return tokenResponse;
        }

        #region Common

        public TResponse CommonGet<TResponse>(string route, string parameters = null)
        {
            return WebClient.SecureGet<TResponse>(Token, _url + route + parameters);
        }

        public TResponse CommonPost<TRequest, TResponse>(string route, TRequest request)
        {
            return WebClient.SecurePostAsJson<TRequest, TResponse>(Token, _url + route, request);
        }

        #endregion

        #region BrandController

        public UserBrandsResponse GetUserBrands()
        {
            return WebClient.SecureGet<UserBrandsResponse>(Token, _url + AdminApiRoutes.GetUserBrands);
        }

        public BrandAddDataResponse GetBrandAddData()
        {
            return WebClient.SecureGet<BrandAddDataResponse>(Token, _url + AdminApiRoutes.GetBrandAddData);
        }

        public BrandEditDataResponse GetBrandEditData(Guid brandId)
        {
            var query = "id=" + brandId;
            return WebClient.SecureGet<BrandEditDataResponse>(Token, _url + AdminApiRoutes.GetBrandEditData, query);
        }

        public BrandViewDataResponse GetBrandViewData(Guid brandId)
        {
            var query = "id=" + brandId;
            return WebClient.SecureGet<BrandViewDataResponse>(Token, _url + AdminApiRoutes.GetBrandViewData, query);
        }

        public AddBrandResponse AddBrand(AddBrandRequest request)
        {
            return WebClient.SecurePostAsJson<AddBrandRequest, AddBrandResponse>(Token, _url + AdminApiRoutes.AddBrand, request);
        }

        public EditBrandResponse EditBrand(EditBrandRequest request)
        {
            return WebClient.SecurePostAsJson<EditBrandRequest, EditBrandResponse>(Token, _url + AdminApiRoutes.EditBrand, request);
        }

        public BrandCountriesResponse GetBrandCountries(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<BrandCountriesResponse>(Token, _url + AdminApiRoutes.GetBrandCountries, query);
        }

        public ActivateBrandResponse ActivateBrand(ActivateBrandRequest request)
        {
            return WebClient.SecurePostAsJson<ActivateBrandRequest, ActivateBrandResponse>(Token, _url + AdminApiRoutes.ActivateBrand, request);
        }

        public DeactivateBrandResponse DeactivateBrand(DeactivateBrandRequest request)
        {
            return WebClient.SecurePostAsJson<DeactivateBrandRequest, DeactivateBrandResponse>(Token, _url + AdminApiRoutes.DeactivateBrand, request);
        }

        public BrandsResponse GetBrands(bool useFilter, Guid[] licensees)
        {
            var query = "useFilter=" + useFilter;
            query += "&licensees=" + String.Join(",", licensees);
            return WebClient.SecureGet<BrandsResponse>(Token, _url + AdminApiRoutes.GetBrands, query);
        }

        #endregion

        #region BrandCountryController

        public BrandCountryAssignDataResponse GetBrandCountryAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<BrandCountryAssignDataResponse>(Token, _url + AdminApiRoutes.GetBrandCountryAssignData, query);
        }

        public AssignBrandCountryResponse AssignBrandCountry(AssignBrandCountryRequest request)
        {
            return WebClient.SecurePostAsJson<AssignBrandCountryRequest, AssignBrandCountryResponse>(Token, _url + AdminApiRoutes.AssignBrandCountry, request);
        }

        #endregion

        #region BrandCultureController

        public BrandCultureAssignDataResponse GetBrandCultureAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<BrandCultureAssignDataResponse>(Token, _url + AdminApiRoutes.GetBrandCultureAssignData, query);
        }

        public AssignBrandCultureResponse AssignBrandCulture(AssignBrandCultureRequest request)
        {
            return WebClient.SecurePostAsJson<AssignBrandCultureRequest, AssignBrandCultureResponse>(Token, _url + AdminApiRoutes.AssignBrandCulture, request);
        }

        #endregion

        #region BrandCurrencyController

        public BrandCurrencyAssignDataResponse GetBrandCurrencyAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<BrandCurrencyAssignDataResponse>(Token, _url + AdminApiRoutes.GetBrandCurrencyAssignData, query);
        }

        public AssignBrandCurrencyResponse AssignBrandCurrency(AssignBrandCurrencyRequest request)
        {
            return WebClient.SecurePostAsJson<AssignBrandCurrencyRequest, AssignBrandCurrencyResponse>(Token, _url + AdminApiRoutes.AssignBrandCurrency, request);
        }

        public GetBrandCurrenciesResponse GetBrandCurrencies(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<GetBrandCurrenciesResponse>(Token, _url + AdminApiRoutes.GetBrandCurrencies, query);
        }

        public GetBrandCurrenciesWithNamesResponse GetBrandCurrenciesWithNames(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<GetBrandCurrenciesWithNamesResponse>(Token, _url + AdminApiRoutes.GetBrandCurrenciesWithNames, query);
        }

        #endregion

        #region BrandProductController

        public HttpResponseMessage GetBrandProductAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetBrandProductAssignData, query);
        }

        public HttpResponseMessage AssignBrandProduct(AssignBrandProductModel request)
        {
            return WebClient.SecurePostAsJson<AssignBrandProductModel, HttpResponseMessage>(Token, _url + AdminApiRoutes.AssignBrandProduct, request);
        }

        public HttpResponseMessage GetBrandProductBetLevels(Guid brandId, Guid productId)
        {
            var query = "brandId=" + brandId;
            query += "&productId=" + productId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetBrandProductBetLevels, query);
        }

        #endregion

        #region ContentTranslationController

        public HttpResponseMessage CreateContentTranslation(AddContentTranslationModel request)
        {
            return WebClient.SecurePostAsJson<AddContentTranslationModel, HttpResponseMessage>(Token, _url + AdminApiRoutes.CreateContentTranslation, request);
        }

        //public HttpResponseMessage UpdateContentTranslation(EditContentTranslationData request)
        //{
        //    return WebClient.SecurePostAsJson<EditContentTranslationData, HttpResponseMessage>(Token, _url + "ContentTranslation/UpdateContentTranslation", request);
        //}

        //public HttpResponseMessage ActivateContentTranslation(ActivateContentTranslationData request)
        //{
        //    return WebClient.SecurePostAsJson<ActivateContentTranslationData, HttpResponseMessage>(Token, _url + "ContentTranslation/Activate", request);
        //}

        //public HttpResponseMessage DeactivateContentTranslation(DeactivateContentTranslationData request)
        //{
        //    return WebClient.SecurePostAsJson<DeactivateContentTranslationData, HttpResponseMessage>(Token, _url + "ContentTranslation/Deactivate", request);
        //}

        //public HttpResponseMessage DeleteContentTranslation(DeleteContentTranslationData request)
        //{
        //    return WebClient.SecurePostAsJson<DeleteContentTranslationData, HttpResponseMessage>(Token, _url + "ContentTranslation/DeleteContentTranslation", request);
        //}

        //public HttpResponseMessage GetContentTranslationAddData()
        //{
        //    return WebClient.SecureGet<HttpResponseMessage>(Token, _url + "ContentTranslation/GetContentTranslationAddData");
        //}

        //public HttpResponseMessage GetContentTranslationEditData(Guid id)
        //{
        //    var query = "id=" + id;
        //    return WebClient.SecureGet<HttpResponseMessage>(Token, _url + "ContentTranslation/GetContentTranslationEditData", query);
        //}

        #endregion

        #region CountryController

        public HttpResponseMessage GetCountryByCode(string code)
        {
            var query = "code=" + code;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetCountryByCode, query);
        }

        public HttpResponseMessage SaveCountry(EditCountryData request)
        {
            return WebClient.SecurePostAsJson<EditCountryData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SaveCountry, request);
        }

        public HttpResponseMessage DeleteCountry(DeleteCountryData request)
        {
            return WebClient.SecurePostAsJson<DeleteCountryData, HttpResponseMessage>(Token, _url + AdminApiRoutes.DeleteCountry, request);
        }

        #endregion

        #region CultureController

        public HttpResponseMessage GetCultureByCode(string code)
        {
            var query = "code=" + code;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetCultureByCode, query);
        }

        public HttpResponseMessage ActivateCulture(ActivateCultureData request)
        {
            return WebClient.SecurePostAsJson<ActivateCultureData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ActivateCulture, request);
        }

        public HttpResponseMessage DeactivateCulture(DeactivateCultureData request)
        {
            return WebClient.SecurePostAsJson<DeactivateCultureData, HttpResponseMessage>(Token, _url + AdminApiRoutes.DeactivateCulture, request);
        }

        public HttpResponseMessage SaveCulture(EditCultureData request)
        {
            return WebClient.SecurePostAsJson<EditCultureData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SaveCulture, request);
        }

        #endregion

        #region CurrencyController

        public HttpResponseMessage GetCurrencyByCode(string code)
        {
            var query = "code=" + code;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetCurrencyByCode, query);
        }

        public HttpResponseMessage ActivateCurrency(ActivateCurrencyData request)
        {
            return WebClient.SecurePostAsJson<ActivateCurrencyData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ActivateCurrency, request);
        }

        public HttpResponseMessage DeactivateCurrency(DeactivateCurrencyData request)
        {
            return WebClient.SecurePostAsJson<DeactivateCurrencyData, HttpResponseMessage>(Token, _url + AdminApiRoutes.DeactivateCurrency, request);
        }

        public HttpResponseMessage SaveCurrency(EditCurrencyData request)
        {
            return WebClient.SecurePostAsJson<EditCurrencyData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SaveCurrency, request);
        }

        #endregion

        #region PlayerManagerController

        public HttpResponseMessage GetAddPlayerDataInPlayerManager()
        {
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetAddPlayerDataInPlayerManager);
        }

        public HttpResponseMessage GetAddPlayerBrandsInPlayerManager(Guid licenseeId)
        {
            var query = "licenseeId=" + licenseeId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetAddPlayerBrandsInPlayerManager, query);
        }

        public HttpResponseMessage GetAddPlayerBrandDataInPlayerManager(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetAddPlayerBrandDataInPlayerManager, query);
        }

        public HttpResponseMessage GetPaymentLevelsInPlayerManager(Guid brandId, string currency)
        {
            var query = "brandId=" + brandId;
            query += "&currency=" + currency;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetPaymentLevelsInPlayerManager, query);
        }

        public HttpResponseMessage GetVipLevelsInPlayerManager(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetVipLevelsInPlayerManager, query);
        }

        public HttpResponseMessage ChangeVipLevelInPlayerManager(ChangeVipLevelData request)
        {
            return WebClient.SecurePostAsJson<ChangeVipLevelData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ChangeVipLevelInPlayerManager, request);
        }

        public HttpResponseMessage ChangePaymnetLevelInPlayerManager(ChangePaymentLevelData request)
        {
            return WebClient.SecurePostAsJson<ChangePaymentLevelData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ChangePaymentLevelInPlayerManager, request);
        }

        public HttpResponseMessage ChangePlayersPaymentLevelInPlayerManager(ChangePlayersPaymentLevelData request)
        {
            return WebClient.SecurePostAsJson<ChangePlayersPaymentLevelData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ChangePlayersPaymentLevelInPlayerManager, request);
        }

        public HttpResponseMessage SendNewPasswordInPlayerManager(SendNewPasswordData request)
        {
            return WebClient.SecurePostAsJson<SendNewPasswordData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SendNewPasswordInPlayerManager, request);
        }

        public HttpResponseMessage AddPlayerInPlayerManager(AddPlayerData request)
        {
            return WebClient.SecurePostAsJson<AddPlayerData, HttpResponseMessage>(Token, _url + AdminApiRoutes.AddPlayerInPlayerManager, request);
        }

        public HttpResponseMessage GetPlayerForBankAccountInPlayerManager(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetPlayerForBankAccountInPlayerManager, query);
        }

        public HttpResponseMessage GetBankAccountInPlayerManager(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetBankAccountInPlayerManager, query);
        }

        public HttpResponseMessage GetCurrentBankAccountInPlayerManager(Guid playerId)
        {
            var query = "playerId=" + playerId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetCurrentBankAccountInPlayerManager, query);
        }

        public HttpResponseMessage SaveBankAccountInPlayerManager(EditPlayerBankAccountData request)
        {
            return WebClient.SecurePostAsJson<EditPlayerBankAccountData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SaveBankAccountInPlayerManager, request);
        }

        public HttpResponseMessage SetCurrentBankAccountInPlayerManager(SetCurrentPlayerBankAccountData request)
        {
            return WebClient.SecurePostAsJson<SetCurrentPlayerBankAccountData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SetCurrentBankAccountInPlayerManager, request);
        }

        #endregion

        #region PlayerInfoController

        public HttpResponseMessage EditLogRemarkInPlayerInfo(EditLogRemarkData request)
        {
            return WebClient.SecurePostAsJson<EditLogRemarkData, HttpResponseMessage>(Token, _url + AdminApiRoutes.EditLogRemarkInPlayerInfo, request);
        }

        //public async Task<HttpResponseMessage> GetBalancesInPlayerInfo(Guid playerId)
        //{
        //    var query = "playerId=" + playerId;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "PlayerInfo/GetBalances", query)).Result;

        //    return await EnsureApiResult<HttpResponseMessage>(result);
        //}

        public HttpResponseMessage GetTransactionTypesInPlayerInfo()
        {
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetTransactionTypesInPlayerInfo);
        }

        //public async Task<HttpResponseMessage> GetWalletTemplatesInPlayerInfo(Guid playerId)
        //{
        //    var query = "playerId=" + playerId;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "PlayerInfo/GetWalletTemplates", query)).Result;

        //    return await EnsureApiResult<HttpResponseMessage>(result);
        //}

        //public async Task<HttpResponseMessage> GetStatusInPlayerInfo(Guid playerId)
        //{
        //    var query = "playerId=" + playerId;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "PlayerInfo/GetStatus", query)).Result;

        //    return await EnsureApiResult<HttpResponseMessage>(result);
        //}

        //public async Task<HttpResponseMessage> GetPlayerTitleInPlayerInfo(Guid id)
        //{
        //    var query = "id=" + id;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "PlayerInfo/GetPlayerTitle", query)).Result;

        //    return await EnsureApiResult<HttpResponseMessage>(result);
        //}

        public HttpResponseMessage GetIdentificationDocumentEditDataInPlayerInfo(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetIdentificationDocumentEditDataInPlayerInfo, query);
        }

        //public HttpResponseMessage UploadIdInPlayerInfo()
        //{
        //    var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "PlayerInfo/UploadId", request)).Result;

        //    return EnsureApiResult<HttpResponseMessage>(result).Result;
        //}

        //public async Task<HttpResponseMessage> GetPlayerInfo(Guid id)
        //{
        //    var query = "id=" + id;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "PlayerInfo/Get", query)).Result;

        //    return await EnsureApiResult<HttpResponseMessage>(result);
        //}

        //public async Task<HttpResponseMessage> GetExemptionDataInPlayerInfo(Guid id)
        //{
        //    var query = "id=" + id;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "PlayerInfo/GetExemptionData", query)).Result;

        //    return await EnsureApiResult<HttpResponseMessage>(result);
        //}

        public HttpResponseMessage EditPlayerInfo(EditPlayerData request)
        {
            return WebClient.SecurePostAsJson<EditPlayerData, HttpResponseMessage>(Token, _url + AdminApiRoutes.EditPlayerInfo, request);
        }

        public HttpResponseMessage SubmitExemptionInPlayerInfo(Exemption request)
        {
            return WebClient.SecurePostAsJson<Exemption, HttpResponseMessage>(Token, _url + AdminApiRoutes.SubmitExemptionInPlayerInfo, request);
        }

        public HttpResponseMessage SetStatusInPlayerInfo(SetStatusData request)
        {
            return WebClient.SecurePostAsJson<SetStatusData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SetStatusInPlayerInfo, request);
        }

        //public HttpResponseMessage VerifyIdDocumentInPlayerInfo(VerifyIdDocumentData request)
        //{
        //    return WebClient.SecurePostAsJson<VerifyIdDocumentData, HttpResponseMessage>(Token, _url + "PlayerInfo/VerifyIdDocument", request);
        //}

        //public HttpResponseMessage UnverifyIdDocumentInPlayerInfo(UnverifyIdDocumentData request)
        //{
        //    return WebClient.SecurePostAsJson<UnverifyIdDocumentData, HttpResponseMessage>(Token, _url + "PlayerInfo/UnverifyIdDocument", request);
        //}

        public HttpResponseMessage ResendActivationEmailInPlayerInfo(ResendActivationEmailData request)
        {
            return WebClient.SecurePostAsJson<ResendActivationEmailData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ResendActivationEmailInPlayerInfo, request);
        }

        #endregion

        #region AdminManagerController

        public HttpResponseMessage CreateUserInAdminManager(AddAdminData request)
        {
            return WebClient.SecurePostAsJson<AddAdminData, HttpResponseMessage>(Token, _url + AdminApiRoutes.CreateUserInAdminManager, request);
        }

        public HttpResponseMessage UpdateUserInAdminManager(EditAdminData request)
        {
            return WebClient.SecurePostAsJson<EditAdminData, HttpResponseMessage>(Token, _url + AdminApiRoutes.UpdateUserInAdminManager, request);
        }

        public HttpResponseMessage ResetPasswordInAdminManager(AddAdminData request)
        {
            return WebClient.SecurePostAsJson<AddAdminData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ResetPasswordInAdminManager, request);
        }

        public HttpResponseMessage ActivateUserInAdminManager(ActivateUserData request)
        {
            return WebClient.SecurePostAsJson<ActivateUserData, HttpResponseMessage>(Token, _url + AdminApiRoutes.ActivateUserInAdminManager, request);
        }

        public HttpResponseMessage DeactivateUserInAdminManager(DeactivateUserData request)
        {
            return WebClient.SecurePostAsJson<DeactivateUserData, HttpResponseMessage>(Token, _url + AdminApiRoutes.DeactivateUserInAdminManager, request);
        }

        public HttpResponseMessage GetUserEditDataInAdminManager(Guid? id = null)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetUserEditDataInAdminManager, query);
        }

        public HttpResponseMessage GetLicenseeDataInAdminManager(GetLicenseeData request)
        {
            return WebClient.SecurePostAsJson<GetLicenseeData, HttpResponseMessage>(Token, _url + AdminApiRoutes.GetLicenseeDataInAdminManager, request);
        }

        public HttpResponseMessage SaveBrandFilterSelectionInAdminManager(BrandFilterSelectionData request)
        {
            return WebClient.SecurePostAsJson<BrandFilterSelectionData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SaveBrandFilterSelectionInAdminManager, request);
        }

        public HttpResponseMessage SaveLicenseeFilterSelectionInAdminManager(LicenseeFilterSelectionData request)
        {
            return WebClient.SecurePostAsJson<LicenseeFilterSelectionData, HttpResponseMessage>(Token, _url + AdminApiRoutes.SaveLicenseeFilterSelectionInAdminManager, request);
        }

        #endregion

        #region RoleManagerController

        public HttpResponseMessage CreateRoleInRoleManager(AddRoleData request)
        {
            return WebClient.SecurePostAsJson<AddRoleData, HttpResponseMessage>(Token, _url + AdminApiRoutes.CreateRoleInRoleManager, request);
        }

        public HttpResponseMessage GetRoleInRoleManager(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetRoleInRoleManager, query);
        }

        public HttpResponseMessage GetEditDataInRoleManager(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetEditDataInRoleManager, query);
        }

        public HttpResponseMessage UpdateRoleInRoleManager(EditRoleData request)
        {
            return WebClient.SecurePostAsJson<EditRoleData, HttpResponseMessage>(Token, _url + AdminApiRoutes.UpdateRoleInRoleManager, request);
        }

        public HttpResponseMessage GetLicenseeDataInRoleManager(IEnumerable<Guid> licensees)
        {
            return WebClient.SecurePostAsJson<IEnumerable<Guid>, HttpResponseMessage>(
                Token,
                _url + AdminApiRoutes.GetLicenseeDataInRoleManager,
                licensees);
        }

        #endregion

        #region IdentificationDocumentSettingsController

        public HttpResponseMessage CreateSettingInIdentificationDocumentSettings(IdentificationDocumentSettingsData request)
        {
            return WebClient.SecurePostAsJson<IdentificationDocumentSettingsData, HttpResponseMessage>(Token, _url + AdminApiRoutes.CreateSettingInIdentificationDocumentSettings, request);
        }

        public HttpResponseMessage GetEditDataInIdentificationDocumentSettings(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetEditDataInIdentificationDocumentSettings, query);
        }

        public HttpResponseMessage UpdateSettingInIdentificationDocumentSettings(IdentificationDocumentSettingsData request)
        {
            return WebClient.SecurePostAsJson<IdentificationDocumentSettingsData, HttpResponseMessage>(Token, _url + AdminApiRoutes.UpdateSettingInIdentificationDocumentSettings, request);
        }

        public HttpResponseMessage GetLicenseeBrandsInIdentificationDocumentSettings(Guid licenseeId)
        {
            var query = "licenseeId=" + licenseeId;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetLicenseeBrandsInIdentificationDocumentSettings, query);
        }

        #endregion

        #region AdminIpRegulationsController

        public string IsIpAddressUniqueInAdminIpRegulations(string ipAddress)
        {
            var query = "ipAddress=" + ipAddress;
            return WebClient.SecureGet<string>(Token, _url + AdminApiRoutes.IsIpAddressUniqueInAdminIpRegulations, query);
        }

        public string IsIpAddressBatchUniqueInAdminIpRegulations(string ipAddressBatch)
        {
            var query = "ipAddressBatch=" + ipAddressBatch;
            return WebClient.SecureGet<string>(Token, _url + AdminApiRoutes.IsIpAddressBatchUniqueInAdminIpRegulations, query);
        }

        public HttpResponseMessage GetEditDataInAdminIpRegulations(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetEditDataInAdminIpRegulations, query);
        }

        public HttpResponseMessage CreateIpRegulationInAdminIpRegulations(EditAdminIpRegulationData request)
        {
            return WebClient.SecurePostAsJson<EditAdminIpRegulationData, HttpResponseMessage>(Token, _url + AdminApiRoutes.CreateIpRegulationInAdminIpRegulations, request);
        }

        public HttpResponseMessage UpdateIpRegulationInAdminIpRegulations(EditAdminIpRegulationData request)
        {
            return WebClient.SecurePostAsJson<EditAdminIpRegulationData, HttpResponseMessage>(Token, _url + AdminApiRoutes.UpdateIpRegulationInAdminIpRegulations, request);
        }

        public HttpResponseMessage DeleteIpRegulationInAdminIpRegulations(DeleteAdminIpRegulationData request)
        {
            return WebClient.SecurePostAsJson<DeleteAdminIpRegulationData, HttpResponseMessage>(Token, _url + AdminApiRoutes.DeleteIpRegulationInAdminIpRegulations, request);
        }

        #endregion

        #region BrandIpRegulationsController

        public string IsIpAddressUniqueInBrandIpRegulations(string ipAddress)
        {
            var query = "ipAddress=" + ipAddress;
            return WebClient.SecureGet<string>(Token, _url + AdminApiRoutes.IsIpAddressUniqueInBrandIpRegulations, query);
        }

        public string IsIpAddressBatchUniqueInBrandIpRegulations(string ipAddressBatch)
        {
            var query = "ipAddressBatch=" + ipAddressBatch;
            return WebClient.SecureGet<string>(Token, _url + AdminApiRoutes.IsIpAddressBatchUniqueInBrandIpRegulations, query);
        }

        public HttpResponseMessage GetLicenseeBrandsInBrandIpRegulations(Guid licenseeId, bool useBrandFilter)
        {
            var query = "licenseeId=" + licenseeId;
            query += "&useBrandFilter=" + useBrandFilter;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetLicenseeBrandsInBrandIpRegulations, query);
        }

        public HttpResponseMessage GetEditDataInBrandIpRegulations(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<HttpResponseMessage>(Token, _url + AdminApiRoutes.GetEditDataInBrandIpRegulations, query);
        }

        public HttpResponseMessage CreateIpRegulationInBrandIpRegulations(AddBrandIpRegulationData request)
        {
            return WebClient.SecurePostAsJson<AddBrandIpRegulationData, HttpResponseMessage>(Token, _url + AdminApiRoutes.CreateIpRegulationInBrandIpRegulations, request);
        }

        public HttpResponseMessage UpdateIpRegulationInBrandIpRegulations(EditBrandIpRegulationData request)
        {
            return WebClient.SecurePostAsJson<EditBrandIpRegulationData, HttpResponseMessage>(Token, _url + AdminApiRoutes.UpdateIpRegulationInBrandIpRegulations, request);
        }

        public HttpResponseMessage DeleteIpRegulationInBrandIpRegulations(DeleteBrandIpRegulationData request)
        {
            return WebClient.SecurePostAsJson<DeleteBrandIpRegulationData, HttpResponseMessage>(Token, _url + AdminApiRoutes.DeleteIpRegulationInBrandIpRegulations, request);
        }

        #endregion

        #region PaymentGatewaySettingsController
        public SavePaymentGatewaySettingsResponse ListPaymentGatewaySettings()//(SearchPackage request)
        {
            //TODO:ONLINEDEPOSIT refractoring
            //return WebClient.SecurePostAsJson<SavePaymentGatewaySettingsRequest, SavePaymentGatewaySettingsResponse>(Token, _url + AdminApiRoutes.ListPaymentGatewaySettings, request);
            return new SavePaymentGatewaySettingsResponse
            {
                Success = true
            };
        }

        public SavePaymentGatewaySettingsResponse AddPaymentGatewaySettings(SavePaymentGatewaySettingsRequest request)
        {
            return WebClient.SecurePostAsJson<SavePaymentGatewaySettingsRequest, SavePaymentGatewaySettingsResponse>(Token, _url + AdminApiRoutes.AddPaymentGatewaySettings, request);
        }

        public SavePaymentGatewaySettingsResponse EditPaymentGatewaySettings(SavePaymentGatewaySettingsRequest request)
        {
            return WebClient.SecurePostAsJson<SavePaymentGatewaySettingsRequest, SavePaymentGatewaySettingsResponse>(Token, _url + AdminApiRoutes.EditPaymentGatewaySettings, request);
        }

        public ActivatePaymentGatewaySettingsResponse ActivatePaymentGatewaySettings(ActivatePaymentGatewaySettingsRequest request)
        {
            return WebClient.SecurePostAsJson<ActivatePaymentGatewaySettingsRequest, ActivatePaymentGatewaySettingsResponse>(Token, _url + AdminApiRoutes.ActivatePaymentGatewaySettings, request);
        }

        public DeactivatePaymentGatewaySettingsResponse DeactivatePaymentGatewaySettings(DeactivatePaymentGatewaySettingsRequest request)
        {
            return WebClient.SecurePostAsJson<DeactivatePaymentGatewaySettingsRequest, DeactivatePaymentGatewaySettingsResponse>(Token, _url + AdminApiRoutes.DeactivatePaymentGatewaySettings, request);
        }

        public GetPaymentGatewaysResponse GetPaymentGateways(GetPaymentGatewaysRequest request)
        {
            return WebClient.SecurePostAsJson<GetPaymentGatewaysRequest, GetPaymentGatewaysResponse>(Token, _url + AdminApiRoutes.GetPaymentGatewaysInPaymentGatewaySettings, request);
        }

        public PaymentGatewaySettingsViewDataResponse GetPaymentGatewaySettingsById(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<PaymentGatewaySettingsViewDataResponse>(Token, _url + AdminApiRoutes.GetPaymentGatewaySettingsById, query);
        }
        #endregion 

        #region OnlineDepositController
        public OnlineDepositViewDataResponse GetOnlineDepositById(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<OnlineDepositViewDataResponse>(Token, _url + AdminApiRoutes.GetOnlineDepositById, query);
        }

        public VerifyOnlineDepositResponse VerifyOnlineDeposit(VerifyOnlineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<VerifyOnlineDepositRequest, VerifyOnlineDepositResponse>(Token, _url + AdminApiRoutes.VerifyOnlineDeposit, request);
        }




        public UnverifyOnlineDepositResponse UnverifyOnlineDeposit(UnverifyOnlineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<UnverifyOnlineDepositRequest, UnverifyOnlineDepositResponse>(Token, _url + AdminApiRoutes.UnverifyOnlineDeposit, request);
        }



        public ApproveOnlineDepositResponse ApproveOnlineDeposit(ApproveOnlineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<ApproveOnlineDepositRequest, ApproveOnlineDepositResponse>(Token, _url + AdminApiRoutes.ApproveOnlineDeposit, request);
        }

        public RejectOnlineDepositResponse RejectOnlineDeposit(RejectOnlineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<RejectOnlineDepositRequest, RejectOnlineDepositResponse>(Token, _url + AdminApiRoutes.RejectOnlineDeposit, request);
        }
        #endregion

        #region OfflineDepositController
        public GetOfflineDepositByIdResponse GetOfflineDepositById(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<GetOfflineDepositByIdResponse>(Token, _url + AdminApiRoutes.GetOfflineDepositById, query);
        }

        public CreateOfflineDepositResponse CreateOfflineDeposit(CreateOfflineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<CreateOfflineDepositRequest, CreateOfflineDepositResponse>(Token, _url + AdminApiRoutes.CreateOfflineDeposit, request);
        }

        public ConfirmOfflineDepositResponse ConfirmOfflineDeposit(ConfirmOfflineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<ConfirmOfflineDepositRequest, ConfirmOfflineDepositResponse>(Token, _url + AdminApiRoutes.ConfirmOfflineDeposit, request);
        }

        public VerifyOfflineDepositResponse VerifyOfflineDeposit(VerifyOfflineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<VerifyOfflineDepositRequest, VerifyOfflineDepositResponse>(Token, _url + AdminApiRoutes.VerifyOfflineDeposit, request);
        }

        public UnverifyOfflineDepositResponse UnverifyOfflineDeposit(UnverifyOfflineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<UnverifyOfflineDepositRequest, UnverifyOfflineDepositResponse>(Token, _url + AdminApiRoutes.UnverifyOfflineDeposit, request);
        }

        public ApproveOfflineDepositResponse ApproveOfflineDeposit(ApproveOfflineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<ApproveOfflineDepositRequest, ApproveOfflineDepositResponse>(Token, _url + AdminApiRoutes.ApproveOfflineDeposit, request);
        }

        public RejectOfflineDepositResponse RejectOfflineDeposit(RejectOfflineDepositRequest request)
        {
            return WebClient.SecurePostAsJson<RejectOfflineDepositRequest, RejectOfflineDepositResponse>(Token, _url + AdminApiRoutes.RejectOfflineDeposit, request);
        }
        #endregion

        #region BonusController
        public ToggleBonusStatusResponse ChangeBonusStatus(ToggleBonusStatus request)
        {
            return WebClient.SecurePostAsJson<ToggleBonusStatus, ToggleBonusStatusResponse>(Token, _url + AdminApiRoutes.ChangeBonusStatus, request);
        }

        public BonusDataResponse GetBonusRelatedData(Guid? id = null)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<BonusDataResponse>(Token, _url + AdminApiRoutes.GetBonusRelatedData, query);
        }

        public AddEditBonusResponse CreateUpdateBonus(CreateUpdateBonus request)
        {
            return WebClient.SecurePostAsJson<CreateUpdateBonus, AddEditBonusResponse>(Token, _url + AdminApiRoutes.CreateUpdateBonus, request);
        }
        #endregion

        #region BonusTemplateController
        public DeleteTemplateResponse DeleteBonusTemplate(DeleteTemplate request)
        {
            return WebClient.SecurePostAsJson<DeleteTemplate, DeleteTemplateResponse>(Token, _url + AdminApiRoutes.DeleteBonusTemplate, request);
        }

        public TemplateDataResponse GetBonusTemplateRelatedData(Guid? id = null)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<TemplateDataResponse>(Token, _url + AdminApiRoutes.GetBonusTemplateRelatedData, query);
        }

        public AddEditTemplateResponse CreateUpdateBonusTemplate(CreateUpdateTemplate request)
        {
            return WebClient.SecurePostAsJson<CreateUpdateTemplate, AddEditTemplateResponse>(Token, _url + AdminApiRoutes.CreateEditBonusTemplate, request);
        }
        #endregion

        #region BonusHistoryController

        public BonusRedemptionResponse GetBonusRedemption(Guid playerId, Guid redemptionId)
        {
            var query = "playerId=" + playerId + "&redemptionId=" + redemptionId;
            return WebClient.SecureGet<BonusRedemptionResponse>(Token, _url + AdminApiRoutes.GetBonusRedemption, query);
        }

        public CancelBonusResponse CancelBonusRedemption(CancelBonusRedemption request)
        {
            return WebClient.SecurePostAsJson<CancelBonusRedemption, CancelBonusResponse>(Token, _url + AdminApiRoutes.CancelBonusRedemption, request);
        }
        #endregion

        #region IssueBonusController

        public IssueTransactionsResponse GetIssueBonusTransactions(Guid playerId, Guid bonusId)
        {
            var query = "playerId=" + playerId + "&bonusId=" + bonusId;
            return WebClient.SecureGet<IssueTransactionsResponse>(Token, _url + AdminApiRoutes.GetIssueBonusTransactions, query);
        }

        public IssueBonusResponse IssueBonus(IssueBonusByCs request)
        {
            return WebClient.SecurePostAsJson<IssueBonusByCs, IssueBonusResponse>(Token, _url + AdminApiRoutes.IssueBonus, request);
        }
        #endregion

        #region BanksController
        public SaveBankResponse AddBank(AddBankRequest request)
        {            
            return WebClient.SecurePostAsJson<AddBankRequest, SaveBankResponse>(Token, _url + AdminApiRoutes.AddBank, request);
        }

        public SaveBankResponse EditBank(EditBankRequest request)
        {
            return WebClient.SecurePostAsJson<EditBankRequest, SaveBankResponse>(Token, _url + AdminApiRoutes.EditBank, request);
        }

        public GetBankByIdResponse GetBankById(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<GetBankByIdResponse>(Token, _url + AdminApiRoutes.GetBankById, query);

        }
        #endregion

        #region BankAccountsController
        public SaveBankAccountResponse AddBankAccount(AddBankAccountRequest request)
        {
            return WebClient.SecurePostAsJson<AddBankAccountRequest, SaveBankAccountResponse>(Token, _url + AdminApiRoutes.AddBankAccount, request);
        }

        public SaveBankAccountResponse EditBankAccount(EditBankAccountRequest request)
        {
            return WebClient.SecurePostAsJson<EditBankAccountRequest, SaveBankAccountResponse>(Token, _url + AdminApiRoutes.EditBankAccount, request);
        }

        public ActivateBankAccountResponse ActivateBankAccount(ActivateBankAccountRequest request)
        {
            return WebClient.SecurePostAsJson<ActivateBankAccountRequest, ActivateBankAccountResponse>(Token, _url + AdminApiRoutes.ActivateBankAccount, request);
        }

        public DeactivateBankAccountResponse DeactivateBankAccount(DeactivateBankAccountRequest request)
        {
            return WebClient.SecurePostAsJson<DeactivateBankAccountRequest, DeactivateBankAccountResponse>(Token, _url + AdminApiRoutes.DeactivateBankAccount, request);
        }

        public GetBankAccountByIdResponse GetBankAccountById(Guid id)
        {
            var query = "id=" + id;
            return WebClient.SecureGet<GetBankAccountByIdResponse>(Token, _url + AdminApiRoutes.GetBankAccountById, query);

        }

        public GetBankAccountCurrencyResponse GetBankAccountCurrencyByUserId(Guid userId)
        {
            var query = "userId=" + userId;
            return WebClient.SecureGet<GetBankAccountCurrencyResponse>(Token, _url + AdminApiRoutes.GetCurrencyByUserId, query);
        }

        public GetBankAccountTypesResponse GetBankAccountTypes()
        {            
            return WebClient.SecureGet<GetBankAccountTypesResponse>(Token, _url + AdminApiRoutes.GetBankAccountTypes);
        }

        public GetBankListByBrandIdResponse GetBankListByBrandId(Guid brandId)
        {
            var query = "brandId=" + brandId;
            return WebClient.SecureGet<GetBankListByBrandIdResponse>(Token, _url + AdminApiRoutes.GetBankListByBrandId, query);
        }

        public GetBankAccountsByPlayerIdResponse GetBankAccountsByPlayerId(Guid playerId)
        {
            var query = "playerId=" + playerId;
            return WebClient.SecureGet<GetBankAccountsByPlayerIdResponse>(Token, _url + AdminApiRoutes.GetBankAccountsByPlayerId, query);
        }
        #endregion

        #region PlayerBankAccountController
        public VerifyPlayerBankAccountResponse VerifyPlayerBankAccount(VerifyPlayerBankAccountRequest request)
        {
            return WebClient.SecurePostAsJson<VerifyPlayerBankAccountRequest, VerifyPlayerBankAccountResponse>(Token, _url + AdminApiRoutes.VerifyPlayerBankAccount, request);
        }

        public RejectPlayerBankAccountResponse RejectPlayerBankAccount(RejectPlayerBankAccountRequest request)
        {
            return WebClient.SecurePostAsJson<RejectPlayerBankAccountRequest, RejectPlayerBankAccountResponse>(Token, _url + AdminApiRoutes.RejectPlayerBankAccount, request);
        }

        #endregion

        #region PaymentLevelController
        public GetPaymentLevelsResponse GetPaymentLevels()
        {
            return WebClient.SecureGet<GetPaymentLevelsResponse>(Token, _url + AdminApiRoutes.GetPaymentLevels);
        }
        #endregion
    }
}