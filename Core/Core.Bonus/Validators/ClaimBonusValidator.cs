using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using FluentValidation;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Data.BonusRedemption;
using Player = AFT.RegoV2.Bonus.Core.Data.Player;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    internal class ClaimBonusValidator : AbstractValidator<ClaimBonusRedemption>
    {
        public ClaimBonusValidator(IBonusRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Func<Guid, Player> playerGetter = playerId => repository.Players.SingleOrDefault(b => b.Id == playerId);
            Func<Guid, Guid, BonusRedemption> redemptionGetter = (playerId, bonusRedemptionId) => 
                repository.Players.Single(b => b.Id == playerId).Wallets.SelectMany(w => w.BonusesRedeemed)
                .SingleOrDefault(br => br.Id == bonusRedemptionId);

            RuleFor(model => model)
                .Must(model => playerGetter(model.PlayerId) != null)
                .WithMessage(ValidatorMessages.PlayerDoesNotExist)
                .Must(model => redemptionGetter(model.PlayerId, model.RedemptionId) != null)
                .WithMessage(ValidatorMessages.BonusRedemptionDoesNotExist)
                .Must(model => redemptionGetter(model.PlayerId, model.RedemptionId).ActivationState == ActivationStatus.Claimable)
                .WithMessage(ValidatorMessages.BonusRedemptionStatusIsIncorrectForClaim)
                .WithName("Model");
        }
    }
}