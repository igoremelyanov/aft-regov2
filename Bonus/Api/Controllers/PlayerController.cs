using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class PlayerController : BaseApiController
    {
        private readonly BonusQueries _bonusQueries;
        private readonly BonusCommands _bonusCommands;

        public PlayerController(BonusQueries bonusQueries, BonusCommands bonusCommands)
        {
            _bonusQueries = bonusQueries;
            _bonusCommands = bonusCommands;
        }

        [HttpGet]
        [Route(Routes.GetWageringBalance)]
        public PlayerWagering GetPlayerWagering(Guid playerId, Guid? walletTemplateId)
        {
            return _bonusQueries.GetWageringBalances(playerId, walletTemplateId);
        }

        [HttpGet]
        [Route(Routes.GetPlayerBalance)]
        public BonusBalance GetPlayerBalance(Guid playerId, Guid? walletTemplateId)
        {
            return _bonusQueries.GetBalance(playerId, walletTemplateId);
        }

        [HttpGet]
        [Route(Routes.GetClaimableRedemptions)]
        public List<BonusRedemption> GetClaimableRedemptions(Guid playerId)
        {
            return _bonusQueries.GetClaimableRedemptions(playerId);
        }

        [HttpGet]
        [Route(Routes.GetDepositQualifiedBonuses)]
        public IEnumerable<DepositQualifiedBonus> GetDepositQualifiedBonuses(Guid playerId, decimal transferAmount)
        {
            return _bonusQueries.GetDepositQualifiedBonuses(playerId, transferAmount);
        }

        [HttpGet]
        [Route(Routes.GetVisibleDepositQualifiedBonuses)]
        public IEnumerable<DepositQualifiedBonus> GetVisibleDepositQualifiedBonuses(Guid playerId, decimal transferAmount)
        {
            return _bonusQueries.GetVisibleDepositQualifiedBonuses(playerId, transferAmount);
        }

        [HttpGet]
        [Route(Routes.GetVisibleDepositQualifiedBonuses)]
        public IEnumerable<DepositQualifiedBonus> GetVisibleDepositQualifiedBonuses(Guid playerId)
        {
            return _bonusQueries.GetVisibleDepositQualifiedBonuses(playerId);
        }

        [HttpGet]
        [Route(Routes.GetDepositQualifiedBonusByCode)]
        public DepositQualifiedBonus GetDepositQualifiedBonusByCode(Guid playerId, string code, decimal transferAmount)
        {
            return _bonusQueries.GetDepositQualifiedBonusByCode(playerId, code, transferAmount);
        }

        [HttpGet]
        [Route(Routes.GetDepositBonusApplicationValidation)]
        public DepositBonusApplicationValidationResponse GetDepositBonusApplicationValidation(Guid playerId, string bonusCode, decimal depositAmount)
        {
            var result = _bonusQueries.GetValidationResult(new FirstDepositApplication
            {
                PlayerId = playerId,
                BonusCode = bonusCode,
                DepositAmount = depositAmount
            });

            return new DepositBonusApplicationValidationResponse
            {
                Success = result.IsValid,
                Errors = result.Errors.Select(er => new ValidationError
                {
                    PropertyName = er.PropertyName,
                    ErrorMessage = er.ErrorMessage
                }).ToList()
            };
        }

        [HttpPost]
        [Route(Routes.ClaimBonusRedemption)]
        public void ClaimBonusRedemption(ClaimBonusRedemption model)
        {
            _bonusCommands.ClaimBonusRedemption(model);
        }

        [HttpPost]
        [Route(Routes.ApplyForFundInBonus)]
        public void ApplyForFundInBonus(FundInBonusApplication model)
        {
            _bonusCommands.ApplyForBonus(model);
        }

        [HttpPost]
        [Route(Routes.ApplyForDepositBonus)]
        public Guid ApplyForDepositBonus(DepositBonusApplication model)
        {
            return _bonusCommands.ApplyForBonus(model);
        }

        [HttpGet]
        [Route(Routes.GetCompletedBonuses)]
        public List<BonusRedemption> GetCompletedBonuses(Guid playerId)
        {
            return _bonusQueries.GetCompletedBonuses(playerId);
        }

        [HttpGet]
        [Route(Routes.GetBonusesWithIncompleteWagering)]
        public List<BonusRedemption> GetBonusesWithInCompleteWagering(Guid playerId)
        {
            return _bonusQueries.GetBonusesWithIncompleteWagering(playerId);
        }
    }
}