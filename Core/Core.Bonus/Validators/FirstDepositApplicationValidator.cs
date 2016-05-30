using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Entities;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using FluentValidation;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    public class FirstDepositApplicationValidator : AbstractValidator<FirstDepositApplication>
    {
        public FirstDepositApplicationValidator(IBonusRepository repository)
        {
            RuleFor(m => m)
                .Must(m => repository.Players.Any(p => p.Id == m.PlayerId))
                .WithMessage(ValidatorMessages.PlayerDoesNotExist)
                .DependentRules(rules =>
                {
                    rules.RuleFor(m => m)
                        .Must(m =>
                        {
                            var bonus = repository.GetLockedBonusOrNull(m.BonusCode);

                            if (bonus != null)
                            {
                                if (bonus.Data.Template.Info.TemplateType != BonusType.FirstDeposit && 
                                bonus.Data.Template.Info.Mode != IssuanceMode.AutomaticWithCode && 
                                bonus.Data.Template.Info.Mode != IssuanceMode.ManualByPlayer)
                                {
                                    return false;
                                }

                                return true;
                            }

                            return false;
                        })
                        .WithMessage(ValidatorMessages.BonusDoesNotExist)
                        .WithName("BonusCode")
                        .DependentRules(dependentRules =>
                        {
                            string error = null;
                            dependentRules.RuleFor(m => m)
                                .Must(m =>
                                {
                                    var bonus = repository.GetLockedBonusOrNull(m.BonusCode);
                                    var player = repository.GetLockedPlayer(m.PlayerId);
                                    var errors = bonus.QualifyFor(player, QualificationPhase.Redemption,
                                        new RedemptionParams { TransferAmount = m.DepositAmount }).ToArray();
                                    if (errors.Any())
                                    {
                                        error = errors.First();
                                        return false;
                                    }

                                    return true;
                                })
                                .WithMessage("{0}", application => error)
                                .WithName("BonusCode");
                        });
                });
        }
    }
}
