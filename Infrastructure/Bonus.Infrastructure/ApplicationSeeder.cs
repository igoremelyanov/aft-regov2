using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Infrastructure
{
    public class ApplicationSeeder
    {
        private readonly BonusManagementCommands _bonusManagementCommands;
        private readonly IBonusRepository _bonusRepository;

        public ApplicationSeeder(BonusManagementCommands bonusManagementCommands, IBonusRepository bonusRepository)
        {
            _bonusManagementCommands = bonusManagementCommands;
            _bonusRepository = bonusRepository;
        }

        public void Seed()
        {
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Manual1", IssuanceMode.ManualByPlayer, BonusRewardType.Amount, 888, 50, 0, "Welcome Bonus", null, "Deposit more than ¥888 to be get extra ¥50");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Manual2", IssuanceMode.ManualByPlayer, BonusRewardType.Percentage, 200, 25, 588, "Join Us Bonus", null, "Deposit more than ¥200 to be eligible for the bonus ");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Manual3", IssuanceMode.ManualByPlayer, BonusRewardType.Percentage, 6666, 50, 8888, "New Player Bonus", null, "Deposit more than ¥6666 to be eligible for the bonus");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Code1", IssuanceMode.AutomaticWithCode, BonusRewardType.Amount, 688, 30, 0, "Only For You Bonus", "Regospecial", "Deposit more than ¥688 to be get extra ¥30");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Code2", IssuanceMode.AutomaticWithCode, BonusRewardType.Percentage, 1200, 10, 200, "Welcome to Rego Bonus", "Regopromotion", "Deposit more than ¥1200 to be eligible for the bonus");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Code3", IssuanceMode.AutomaticWithCode, BonusRewardType.Percentage, 3888, 20, 1888, "New Account Offer", "Regocode", "Deposit more than ¥3888 to be eligible for the bonus");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Auto1", IssuanceMode.Automatic, BonusRewardType.Amount, 1688, 600, 0, "Welcome Bonus for you", null, "Deposit more than ¥1688 to be get extra ¥600");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Auto2", IssuanceMode.Automatic, BonusRewardType.Percentage, 2666, 15, 6888, "Special Bonus for New Player", null, "Deposit more than ¥2666 to be eligible for the bonus");
            AddBonus(BonusType.FirstDeposit, "First Time Deposit_Auto3", IssuanceMode.Automatic, BonusRewardType.Percentage, 5888, 30, 8888, "New account Bonus", null, "Deposit more than ¥5888 to be eligible for the bonus");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Manual1", IssuanceMode.ManualByPlayer, BonusRewardType.Amount, 888, 50, 0, "Reload Bonus", null, "Deposit more than ¥888 to be get extra ¥50");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Manual2", IssuanceMode.ManualByPlayer, BonusRewardType.Percentage, 200, 25, 588, "Deposit Bonus", null, "Deposit more than ¥200 to be eligible for the bonus ");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Manual3", IssuanceMode.ManualByPlayer, BonusRewardType.Percentage, 6666, 50, 8888, "Special Deposit Bonus", null, "Deposit more than ¥6666 to be eligible for the bonus");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Code1", IssuanceMode.AutomaticWithCode, BonusRewardType.Amount, 688, 30, 0, "Play More Bonus", "Regoreload", "Deposit more than ¥688 to be get extra ¥30");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Code2", IssuanceMode.AutomaticWithCode, BonusRewardType.Percentage, 1200, 10, 200, "Keep Playing Bonus", "Regohead", "Deposit more than ¥1200 to be eligible for the bonus");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Code3", IssuanceMode.AutomaticWithCode, BonusRewardType.Percentage, 3888, 20, 1888, "Win More Bonus", "Regoproject", "Deposit more than ¥3888 to be eligible for the bonus");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Auto1", IssuanceMode.Automatic, BonusRewardType.Amount, 1688, 600, 0, "Deposit Reward Bonus", null, "Deposit more than ¥1688 to be get extra ¥600");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Auto2", IssuanceMode.Automatic, BonusRewardType.Percentage, 2666, 15, 6888, "Exclusive Bonus", null, "Deposit more than ¥2666 to be eligible for the bonus");
            AddBonus(BonusType.ReloadDeposit, "ReloadDeposit_Auto3", IssuanceMode.Automatic, BonusRewardType.Percentage, 5888, 30, 8888, "Exclusive Reward Bonus", null, "Deposit more than ¥5888 to be eligible for the bonus");
        }

        public void AddBonus(
            BonusType bonusType,
            string templateName,
            IssuanceMode mode,
            BonusRewardType rewardType,
            decimal depositAmountFrom,
            decimal bonusAmount,
            decimal maxBonus,
            string bonusName,
            string bonusCode,
            string description)
        {
            var brand = _bonusRepository.Brands.Single(b => b.Id == new Guid("00000000-0000-0000-0000-000000000138"));
            Guid templateId;
            var template = _bonusRepository.Templates.SingleOrDefault(t => t.Info.Name == templateName);
            if (template == null)
            {
                var model = new CreateUpdateTemplate
                {
                    Id = Guid.Empty,
                    Info = new CreateUpdateTemplateInfo
                    {
                        Name = templateName,
                        TemplateType = bonusType,
                        BrandId = brand.Id,
                        WalletTemplateId = brand.WalletTemplates.First().Id,
                        Mode = mode
                    }
                };
                var identifier = _bonusManagementCommands.AddUpdateTemplate(model);
                templateId = identifier.Id;

                model = new CreateUpdateTemplate
                {
                    Id = identifier.Id,
                    Version = identifier.Version,
                    Availability = new CreateUpdateTemplateAvailability(),
                    Rules = new CreateUpdateTemplateRules
                    {
                        RewardType = rewardType,
                        RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "RMB",
                            BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier
                            {
                                From = depositAmountFrom,
                                Reward = bonusAmount
                            }},
                            RewardAmountLimit = maxBonus
                        }
                    }
                    },
                    Wagering = new CreateUpdateTemplateWagering
                    {
                        HasWagering = true,
                        Method = WageringMethod.Bonus,
                        Multiplier = 2
                    },
                    Notification = new CreateUpdateTemplateNotification()
                };
                _bonusManagementCommands.AddUpdateTemplate(model);
            }
            else
            {
                templateId = template.Id;
            }

            var bonus = _bonusRepository.Bonuses.SingleOrDefault(b => b.Name == bonusName);
            if (bonus == null)
            {
                var startDate = new DateTime(2016, 4, 26, 0, 0, 0);
                var endDate = new DateTime(2016, 12, 31, 0, 0, 0);
                var bonusId = _bonusManagementCommands.AddUpdateBonus(new CreateUpdateBonus
                {
                    Id = Guid.Empty,
                    Name = bonusName,
                    Code = bonusCode,
                    Description = description,
                    TemplateId = templateId,
                    ActiveFrom = startDate,
                    ActiveTo = endDate,
                    DurationStart = startDate,
                    DurationEnd = endDate
                });

                if (bonusType == BonusType.FirstDeposit && mode == IssuanceMode.Automatic)
                    return;

                _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus
                {
                    Id = bonusId,
                    IsActive = true
                });
            }
        }
    }
}