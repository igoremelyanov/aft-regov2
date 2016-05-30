using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Common.EventStore.Data;
using AFT.RegoV2.Shared.ApiDataFiltering;
using Template = AFT.RegoV2.Bonus.Core.Models.Data.Template;

namespace AFT.RegoV2.Bonus.Api.Interface.Proxy
{
    public class ApiProxy : OAuthProxy, IBonusApiProxy
    {
        public ApiProxy(HttpClient httpClient, string clientId, string clientSecret, Guid actorId) : base(httpClient, clientId, clientSecret, actorId)
        {
        }

        public Task<FilteredDataResponse<Core.Models.Data.Bonus>> GetFilteredBonusesAsync(BrandFilteredDataRequest request)
        {
            return SecurePostAsJson<BrandFilteredDataRequest, FilteredDataResponse<Core.Models.Data.Bonus>>(Routes.ListBonuses, request);
        }

        public Task<Core.Models.Data.Bonus> GetBonusOrNull(Guid bonusId)
        {
            var query = "bonusId=" + bonusId;
            return SecureGet<Core.Models.Data.Bonus>(Routes.GetBonusOrNull, query);
        }

        public Task<ToggleBonusStatusResponse> ChangeBonusStatusAsync(ToggleBonusStatus request)
        {
            return SecurePostAsJson<ToggleBonusStatus, ToggleBonusStatusResponse>(Routes.ChangeBonusStatus, request);
        }

        public Task<List<BonusTemplate>> GetCompletedTemplatesAsync()
        {
            return SecureGet<List<BonusTemplate>>(Routes.GetCompletedTemplates, string.Empty);
        }

        public Task<AddEditBonusResponse> CreateUpdateBonusAsync(CreateUpdateBonus request)
        {
            return SecurePostAsJson<CreateUpdateBonus, AddEditBonusResponse>(Routes.CreateUpdateBonus, request);
        }

        public Task<FilteredDataResponse<TemplateSummary>> GetFilteredBonusTemplatesAsync(BrandFilteredDataRequest request)
        {
            return SecurePostAsJson<BrandFilteredDataRequest, FilteredDataResponse<TemplateSummary>>(Routes.ListBonusTemplates, request);
        }

        public Task<Template> GetTemplateOrNull(Guid templateId)
        {
            var query = "templateId=" + templateId;
            return SecureGet<Template>(Routes.GetTemplateOrNull, query);
        }

        public Task<DeleteTemplateResponse> DeleteBonusTemplateAsync(DeleteTemplate request)
        {
            return SecurePostAsJson<DeleteTemplate, DeleteTemplateResponse>(Routes.DeleteBonusTemplate, request);
        }

        public Task<List<BonusData>> GetBonusesAsync()
        {
            return SecureGet<List<BonusData>>(Routes.GetBonuses, string.Empty);
        }

        public Task<AddEditTemplateResponse> CreateUpdateBonusTemplateAsync(CreateUpdateTemplate request)
        {
            return SecurePostAsJson<CreateUpdateTemplate, AddEditTemplateResponse>(Routes.CreateEditBonusTemplate, request);
        }

        public Task<FilteredDataResponse<BonusRedemption>> GetFilteredBonusRedemptionAsync(PlayerFilteredDataRequest request)
        {
            return SecurePostAsJson<PlayerFilteredDataRequest, FilteredDataResponse<BonusRedemption>>(Routes.ListBonusRedemptions, request);
        }

        public Task<FilteredDataResponse<Event>> GetFilteredBonusRedemptionEventsAsync(RedemptionFilteredDataRequest request)
        {
            return SecurePostAsJson<RedemptionFilteredDataRequest, FilteredDataResponse<Event>>(Routes.ListRedemptionEvents, request);
        }

        public Task<BonusRedemption> GetBonusRedemptionAsync(Guid playerId, Guid redemptionId)
        {
            var query = $"playerId={playerId}&redemptionId={redemptionId}";
            return SecureGet<BonusRedemption>(Routes.GetBonusRedemption, query);
        }

        public Task<CancelBonusResponse> CancelBonusRedemptionAsync(CancelBonusRedemption request)
        {
            return SecurePostAsJson<CancelBonusRedemption, CancelBonusResponse>(Routes.CancelBonusRedemption, request);
        }

        public Task<FilteredDataResponse<ManualByCsQualifiedBonus>> GetFilteredIssueBonusesAsync(PlayerFilteredDataRequest request)
        {
            return SecurePostAsJson<PlayerFilteredDataRequest, FilteredDataResponse<ManualByCsQualifiedBonus>>(Routes.ListIssueBonuses, request);
        }

        public Task<List<QualifiedTransaction>> GetIssueBonusTransactionsAsync(Guid playerId, Guid bonusId)
        {
            var query = "playerId=" + playerId + "&bonusId=" + bonusId;
            return SecureGet<List<QualifiedTransaction>>(Routes.GetIssueBonusTransactions, query);
        }

        public Task<IssueBonusResponse> IssueBonusAsync(IssueBonusByCs request)
        {
            return SecurePostAsJson<IssueBonusByCs, IssueBonusResponse>(Routes.IssueBonus, request);
        }

        public Task<PlayerWagering> GetWageringBalancesAsync(Guid playerId, Guid? walletTemplateId = null)
        {
            var query = "playerId=" + playerId + "&walletTemplateId=" + walletTemplateId;
            return SecureGet<PlayerWagering>(Routes.GetWageringBalance, query);
        }
        public Task<BonusBalance> GetPlayerBalanceAsync(Guid playerId, Guid? walletTemplateId = null)
        {
            var query = "playerId=" + playerId + "&walletTemplateId=" + walletTemplateId;
            return SecureGet<BonusBalance>(Routes.GetPlayerBalance, query);
        }

        public Task<List<DepositQualifiedBonus>> GetDepositQualifiedBonusesByAdminAsync(Guid playerId)
        {
            var query = "playerId=" + playerId;
            return SecureGet<List<DepositQualifiedBonus>>(Routes.GetDepositQualifiedBonusesByAdmin, query);
        }

        public Task<List<BonusRedemption>> GetClaimableRedemptionsAsync(Guid playerId)
        {
            var query = "playerId=" + playerId;
            return SecureGet<List<BonusRedemption>>(Routes.GetClaimableRedemptions, query);
        }

        public Task<List<DepositQualifiedBonus>> GetDepositQualifiedBonusesAsync(Guid playerId, decimal transferAmount = 0)
        {
            var query = $"playerId={playerId}&transferAmount={transferAmount.ToString(CultureInfo.InvariantCulture)}";
            return SecureGet<List<DepositQualifiedBonus>>(Routes.GetDepositQualifiedBonuses, query);
        }

        public Task<List<DepositQualifiedBonus>> GetDepositQualifiedBonusesAsync(Guid playerId)
        {
            var query = $"playerId={playerId}";
            return SecureGet<List<DepositQualifiedBonus>>(Routes.GetDepositQualifiedBonuses, query);
        }

        public Task<List<DepositQualifiedBonus>> GetVisibleDepositQualifiedBonuses(Guid playerId, decimal transferAmount = 0)
        {
            var query = $"playerId={playerId}&transferAmount={transferAmount.ToString(CultureInfo.InvariantCulture)}";
            return SecureGet<List<DepositQualifiedBonus>>(Routes.GetVisibleDepositQualifiedBonuses, query);
        }

        public Task<List<DepositQualifiedBonus>> GetVisibleDepositQualifiedBonuses(Guid playerId)
        {
            var query = $"playerId={playerId}";
            return SecureGet<List<DepositQualifiedBonus>>(Routes.GetVisibleDepositQualifiedBonuses, query);
        }

        public Task<DepositQualifiedBonus> GetDepositQualifiedBonusByCodeAsync(Guid playerId, string code, decimal transferAmount)
        {
            var query = $"playerId={playerId}&code={code}&transferAmount={transferAmount.ToString(CultureInfo.InvariantCulture)}";
            return SecureGet<DepositQualifiedBonus>(Routes.GetDepositQualifiedBonusByCode, query);
        }

        public Task<DepositBonusApplicationValidationResponse> GetDepositBonusApplicationValidationAsync(Guid playerId, string bonusCode, decimal depositAmount)
        {
            var query = $"playerId={playerId}&bonusCode={bonusCode}&depositAmount={depositAmount.ToString(CultureInfo.InvariantCulture)}";
            return SecureGet<DepositBonusApplicationValidationResponse>(Routes.GetDepositBonusApplicationValidation, query);
        }

        public Task ClaimBonusRedemptionAsync(ClaimBonusRedemption model)
        {
            return SecurePostAsJson(Routes.ClaimBonusRedemption, model);
        }

        public Task ApplyForBonusAsync(FundInBonusApplication model)
        {
            return SecurePostAsJson(Routes.ApplyForFundInBonus, model);
        }
        public Task<Guid> ApplyForBonusAsync(DepositBonusApplication model)
        {
            return SecurePostAsJson<DepositBonusApplication, Guid>(Routes.ApplyForDepositBonus, model);
        }

        public Task<List<BonusRedemption>> GetCompletedBonusesAsync(Guid playerId)
        {
            var query = $"playerId={playerId}";
            return SecureGet<List<BonusRedemption>>(Routes.GetCompletedBonuses, query);
        }
        public Task<List<BonusRedemption>> GetBonusesWithIncompleteWageringAsync(Guid playerId)
        {
            var query = $"playerId={playerId}";
            return SecureGet<List<BonusRedemption>>(Routes.GetBonusesWithIncompleteWagering, query);
        }

        /// <summary>
        /// This methods allows to ensure that player is created in Bonus subdomain or wait some time waiting this creation. 
        /// It is required in some scenarios where requests to Bonus API could be made right after player registration
        /// </summary>
        /// <param name="playerId">Id of player to be awaited</param>
        /// <param name="timeout">Time in milliseconds before the timeout</param>
        /// <returns></returns>
        public Task EnsureOrWaitPlayerRegistered(Guid playerId, int timeout)
        {
            var query = $"playerId={playerId}&timeout={timeout}";
            return SecureGet(Routes.EnsureOrWaitUserRegistered, query);
        }
    }
}