using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.Shared;
using BonusRedemption = AFT.RegoV2.MemberApi.Interface.Bonus.BonusRedemption;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class BonusController : BaseApiController
    {
        private readonly IBonusApiProxy _bonusApiProxy;
        private static readonly string UriRootToRedemptionInfo = "api/Bonus/GetRedemption?redemptionId=";

        public BonusController(IBonusApiProxy bonusApiProxy)
        {
            _bonusApiProxy = bonusApiProxy;
        }

        [HttpGet]
        public async Task<BonusRedemptionsResponse> BonusRedemptions()
        {
            var redemptions = await _bonusApiProxy.GetClaimableRedemptionsAsync(PlayerId);

            return new BonusRedemptionsResponse
            {
                Redemptions = redemptions
                .OrderBy(r => r.CreatedOn)
                .Select(a => new ClaimableRedemption
                {
                    Id = a.Id,
                    BonusName = a.Bonus.Name,
                    Amount = a.Amount,
                    Description = a.Bonus.Description,
                    Code = a.Bonus.Code,
                    State = (int)(a.IsExpired ? ClaimableRedemptionState.Expired : ClaimableRedemptionState.Active),
                    ClaimableFrom = a.ClaimableFrom.ToString("g"),
                    ClaimableTo = a.ClaimableTo.ToString("g")
                })
                .ToArray()
            };
        }

        [HttpGet]
        public async Task<Bonus.Core.Models.Data.BonusRedemption> GetRedemption(Guid redemptionId)
        {
            var redemption = await _bonusApiProxy.GetBonusRedemptionAsync(PlayerId, redemptionId);
            if (redemption == null)
                throw new RegoValidationException(ErrorMessagesEnum.RedemptionWithSuchIdDoesntExist.ToString());

            return redemption;
        }

        [HttpGet]
        public async Task<IEnumerable<BonusRedemption>> GetCompleteBonuses([FromUri]CompleteBonusesRequest request)
        {
            var result = await _bonusApiProxy.GetCompletedBonusesAsync(PlayerId);

            if (result == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            if (request.StartDate.HasValue)
                result = result.Where(x => x.CreatedOn >= request.StartDate.Value).ToList();

            if (request.EndDate.HasValue)
                result = result.Where(x => x.CreatedOn <= request.EndDate.Value).ToList();

            return result.Select(Convert);
        }

        [HttpGet]
        public async Task<IEnumerable<BonusRedemption>> GetBonusesWithInCompleteWagering()
        {
            var result = await _bonusApiProxy.GetBonusesWithIncompleteWageringAsync(PlayerId);
            if (result == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return result.Select(Convert);
        }

        [HttpGet]
        public async Task<IEnumerable<QualifiedBonus>> GetQualifiedBonuses([FromUri]QualifiedBonusRequest request)
        {
            var bonuses = new List<DepositQualifiedBonus>();
            if (request.Amount.HasValue)
            {
                bonuses = await _bonusApiProxy.GetDepositQualifiedBonusesAsync(PlayerId, request.Amount.Value);
            }
            else
            {
                bonuses = await _bonusApiProxy.GetDepositQualifiedBonusesAsync(PlayerId);
            }

            return bonuses
                .Where(x => x.BonusAmount > 0)
                .Select(Convert);
        }

        [HttpPost]
        public async Task<CreatedNegotiatedContentResult<ClaimRedemptionResponse>> ClaimRedemption(ClaimRedemptionRequest request)
        {
            await _bonusApiProxy.ClaimBonusRedemptionAsync(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = request.RedemptionId
            });

            var uri = UriRootToRedemptionInfo + request.RedemptionId;

            return Created(uri, new ClaimRedemptionResponse()
            {
                UriToClaimedRedemption = UriRootToRedemptionInfo + request.RedemptionId
            });
        }

        [HttpGet]
        public async Task<IEnumerable<QualifiedBonus>> QualifiedFirstDepositBonuses([FromUri]QualifiedBonusRequest request)
        {
            List<DepositQualifiedBonus> bonuses;

            if (request.Amount.HasValue)
                bonuses = await _bonusApiProxy.GetDepositQualifiedBonusesAsync(PlayerId, request.Amount.Value);
            else
                bonuses = await _bonusApiProxy.GetDepositQualifiedBonusesAsync(PlayerId);
            return bonuses.Select(Convert);
        }

        [HttpGet]
        public async Task<IEnumerable<QualifiedBonus>> GetVisibleDepositQualifiedBonuses([FromUri]QualifiedBonusRequest request)
        {
            List<DepositQualifiedBonus> bonuses;
            await _bonusApiProxy.EnsureOrWaitPlayerRegistered(PlayerId);
            if (request.Amount.HasValue)
                bonuses = await _bonusApiProxy.GetVisibleDepositQualifiedBonuses(PlayerId, request.Amount.Value);
            else
                bonuses = await _bonusApiProxy.GetVisibleDepositQualifiedBonuses(PlayerId);

            if (bonuses == null)
                throw new RegoValidationException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return bonuses.Select(Convert);
        }

        [HttpGet]
        public async Task<QualifiedBonus> GetFirstDepositBonuseByCode([FromUri]FirstDepositApplicationRequest request)
        {
            var bonus = await _bonusApiProxy
                .GetDepositQualifiedBonusByCodeAsync(PlayerId, request.BonusCode, request.DepositAmount);
            if (bonus == null)
                throw new RegoValidationException(ErrorMessagesEnum.BonusForSuchCodeDoesntExist.ToString());

            return Convert(bonus);
        }

        private QualifiedBonus Convert(DepositQualifiedBonus bonus)
        {
            if (bonus == null)
                return null;

            return new QualifiedBonus
            {
                Id = bonus.Id,
                Name = bonus.Name,
                Code = bonus.Code,
                Description = bonus.Description,
                Percentage = bonus.Percenage,
                RequiredAmount = bonus.RequiredAmount,
                Amount = Math.Round(bonus.BonusAmount, 1)
            };
        }

        protected BonusRedemption Convert(Bonus.Core.Models.Data.BonusRedemption redemption)
        {
            return new BonusRedemption
            {
                Id = redemption.Id,
                Name = redemption.Bonus.Name,
                Code = redemption.Bonus.Code,
                ActivationState = redemption.ActivationState.ToString(),
                Reward = redemption.Amount,
                LockedAmount = redemption.LockedAmount,
                Rollover = redemption.Rollover,
                RolloverLeft = redemption.RolloverLeft,
                CreatedOn = redemption.CreatedOn
            };
        }

        [HttpGet]
        public async Task<FirstDepositApplicationResponse> ValidateFirstOnlineDeposit([FromUri]FirstDepositApplicationRequest request)
        {
            var validationResult = await _bonusApiProxy.GetDepositBonusApplicationValidationAsync(PlayerId, request.BonusCode, request.DepositAmount);
            var isNotQualifiedByAmount = !validationResult.Success && validationResult.Errors
                .Any(o => o.ErrorMessage == QualificationReasons.DepositAmountIsNotQualifiedForTheBonus);

            var bonus = await _bonusApiProxy.GetDepositQualifiedBonusByCodeAsync(PlayerId, request.BonusCode, request.DepositAmount);

            if (isNotQualifiedByAmount)
                return new FirstDepositApplicationResponse
                {
                    IsValid = false,
                    Errors = new[] { $"Sorry, you have to deposit at least ${bonus.RequiredAmount}" }
                };

            if (!validationResult.Success)
                return new FirstDepositApplicationResponse
                {
                    IsValid = false,
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage)
                };

            return new FirstDepositApplicationResponse
            {
                IsValid = true,
                Bonus = Convert(bonus)
            };
        }
    }
}