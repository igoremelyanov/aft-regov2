using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Entities;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using FluentValidation;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    public class DepositBonusApplicationValidator : AbstractValidator<DepositBonusApplication>
    {
        public DepositBonusApplicationValidator(IBonusRepository repository)
        {
            RuleFor(m => m.PlayerId)
                .Must(playerId => repository.Players.Any(p => p.Id == playerId))
                .WithMessage(ValidatorMessages.PlayerDoesNotExist)
                .DependentRules(rules =>
                {
                    rules.RuleFor(m => m)
                        .Must(m =>
                        {
                            var bonus = m.BonusId.HasValue
                                ? repository.GetLockedBonusOrNull(m.BonusId.Value)
                                : repository.GetLockedBonusOrNull(m.BonusCode);

                            if (bonus != null)
                            {
                                if ((bonus.Data.Template.Info.TemplateType != BonusType.FirstDeposit &&
                                     bonus.Data.Template.Info.TemplateType != BonusType.ReloadDeposit) ||
                                    (bonus.Data.Template.Info.Mode != IssuanceMode.AutomaticWithCode &&
                                     bonus.Data.Template.Info.Mode != IssuanceMode.ManualByPlayer))
                                {
                                    return false;
                                }

                                return true;
                            }

                            return false;
                        })
                        .WithMessage(ValidatorMessages.BonusDoesNotExist)
                        .WithName("Model")
                        .DependentRules(dependentRules =>
                        {
                            string error = null;
                            dependentRules.RuleFor(m => m)
                                .Must(m =>
                                {
                                    var bonus = m.BonusId.HasValue
                                        ? repository.GetLockedBonus(m.BonusId.Value)
                                        : repository.GetLockedBonus(m.BonusCode);

                                    var player = repository.GetLockedPlayer(m.PlayerId);
                                    var errors = bonus.QualifyFor(player, QualificationPhase.Redemption,
                                        new RedemptionParams { TransferAmount = m.Amount }).ToArray();
                                    if (errors.Any())
                                    {
                                        error = errors.First();
                                        return false;
                                    }

                                    return true;
                                })
                                .WithMessage("{0}", application => error)
                                .WithName("Model");
                        });
                });

            RuleFor(m => m.DepositId)
                .NotEmpty()
                .WithMessage(ValidatorMessages.DepositIdIsEmpty);
        }
    }
}