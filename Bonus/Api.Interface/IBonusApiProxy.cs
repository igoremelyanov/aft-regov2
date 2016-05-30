using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Common.EventStore.Data;
using AFT.RegoV2.Shared.ApiDataFiltering;

namespace AFT.RegoV2.Bonus.Api.Interface
{
    public interface IBonusApiProxy
    {
        Task<FilteredDataResponse<Core.Models.Data.Bonus>> GetFilteredBonusesAsync(BrandFilteredDataRequest request);
        Task<Core.Models.Data.Bonus> GetBonusOrNull(Guid bonusId);
        Task<ToggleBonusStatusResponse> ChangeBonusStatusAsync(ToggleBonusStatus request);
        Task<List<BonusData>> GetBonusesAsync();
        Task<AddEditBonusResponse> CreateUpdateBonusAsync(CreateUpdateBonus request);

        Task<FilteredDataResponse<TemplateSummary>> GetFilteredBonusTemplatesAsync(BrandFilteredDataRequest request);
        Task<Template> GetTemplateOrNull(Guid templateId);
        Task<List<BonusTemplate>> GetCompletedTemplatesAsync();
        Task<DeleteTemplateResponse> DeleteBonusTemplateAsync(DeleteTemplate request);
        Task<AddEditTemplateResponse> CreateUpdateBonusTemplateAsync(CreateUpdateTemplate request);

        Task<FilteredDataResponse<BonusRedemption>> GetFilteredBonusRedemptionAsync(PlayerFilteredDataRequest request);
        Task<FilteredDataResponse<Event>> GetFilteredBonusRedemptionEventsAsync(RedemptionFilteredDataRequest request);
        Task<BonusRedemption> GetBonusRedemptionAsync(Guid playerId, Guid redemptionId);
        Task<CancelBonusResponse> CancelBonusRedemptionAsync(CancelBonusRedemption request);

        Task<FilteredDataResponse<ManualByCsQualifiedBonus>> GetFilteredIssueBonusesAsync(PlayerFilteredDataRequest request);
        Task<List<QualifiedTransaction>> GetIssueBonusTransactionsAsync(Guid playerId, Guid bonusId);
        Task<IssueBonusResponse> IssueBonusAsync(IssueBonusByCs request);

        Task<PlayerWagering> GetWageringBalancesAsync(Guid playerId, Guid? walletTemplateId = null);
        Task<BonusBalance> GetPlayerBalanceAsync(Guid playerId, Guid? walletTemplateId = null);

        Task<List<DepositQualifiedBonus>> GetDepositQualifiedBonusesByAdminAsync(Guid playerId);
        Task<List<DepositQualifiedBonus>> GetDepositQualifiedBonusesAsync(Guid playerId, decimal transferAmount = 0);
        Task<List<DepositQualifiedBonus>> GetDepositQualifiedBonusesAsync(Guid playerId);
        Task<List<DepositQualifiedBonus>> GetVisibleDepositQualifiedBonuses(Guid playerId, decimal transferAmount = 0);
        Task<List<DepositQualifiedBonus>> GetVisibleDepositQualifiedBonuses(Guid playerId);
        Task<DepositQualifiedBonus> GetDepositQualifiedBonusByCodeAsync(Guid playerId, string code, decimal transferAmount);
        Task<DepositBonusApplicationValidationResponse> GetDepositBonusApplicationValidationAsync(Guid playerId, string bonusCode, decimal depositAmount);

        Task ApplyForBonusAsync(FundInBonusApplication model);
        Task<Guid> ApplyForBonusAsync(DepositBonusApplication model);

        Task<List<BonusRedemption>> GetClaimableRedemptionsAsync(Guid playerId);
        Task ClaimBonusRedemptionAsync(ClaimBonusRedemption model);

        Task<List<BonusRedemption>> GetCompletedBonusesAsync(Guid playerId);
        Task<List<BonusRedemption>> GetBonusesWithIncompleteWageringAsync(Guid playerId);

        Task EnsureOrWaitPlayerRegistered(Guid playerId, int timeout = 5 * 30 * 1000);
    }
}