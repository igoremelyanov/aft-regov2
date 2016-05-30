using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;
using RewardTier = AFT.RegoV2.Bonus.Core.Data.RewardTier;
using Template = AFT.RegoV2.Bonus.Core.Data.Template;
using TemplateAvailability = AFT.RegoV2.Bonus.Core.Data.TemplateAvailability;
using TemplateInfo = AFT.RegoV2.Bonus.Core.Data.TemplateInfo;
using TemplateNotification = AFT.RegoV2.Bonus.Core.Data.TemplateNotification;
using TemplateRules = AFT.RegoV2.Bonus.Core.Data.TemplateRules;
using TemplateWagering = AFT.RegoV2.Bonus.Core.Data.TemplateWagering;

namespace AFT.RegoV2.Bonus.Tests.Unit.Validation
{
    internal class TemplateValidationTests : UnitTestBase
    {
        private Template _template;
        private CreateUpdateTemplate _model;
        private CreateUpdateTemplateRules _rules;
        private CreateUpdateTemplateInfo _info;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var brand = BonusRepository.Brands.First();
            _template = new Template
            {
                Status = TemplateStatus.Complete,
                Info = new TemplateInfo
                {
                    TemplateType = BonusType.FirstDeposit,
                    Name = TestDataGenerator.GetRandomString(),
                    Brand = brand,
                    WalletTemplateId = brand.WalletTemplates.First().Id
                },
                Availability = new TemplateAvailability(),
                Rules = new TemplateRules
                {
                    RewardTiers = new List<RewardTier>
                    {
                        new RewardTier
                        {
                            CurrencyCode = brand.Currencies.First().Code,
                            BonusTiers = new List<TierBase>
                            {
                                new TierBase {Reward = 100}
                            }
                        }
                    }
                },
                Wagering = new TemplateWagering(),
                Notification = new TemplateNotification()
            };

            BonusRepository.Templates.Add(_template);

            _model = new CreateUpdateTemplate
            {
                Id = _template.Id,
                Version = _template.Version
            };
            _info = new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                BrandId = _template.Info.Brand.Id,
                WalletTemplateId = _template.Info.Brand.WalletTemplates.First().Id,
                TemplateType = BonusType.FirstDeposit,
                Mode = IssuanceMode.Automatic
            };
            _rules = new CreateUpdateTemplateRules
            {
                RewardTiers = new List<CreateUpdateRewardTier>
                {
                    new CreateUpdateRewardTier
                    {
                        CurrencyCode = "CAD",
                        BonusTiers = new List<CreateUpdateTemplateTier>
                        {
                            new CreateUpdateTemplateTier {Reward = 25}
                        }
                    }
                }
            };
        }

        [Test]
        public void Name_cant_be_empty()
        {
            _info.Name = string.Empty;
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotSpecified, message);
        }

        [TestCase(50, true)]
        [TestCase(51, false)]
        public void Name_length_validation_works(int length, bool isValid)
        {
            _info.Name = TestDataGenerator.GetRandomString(length);
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(isValid, validationResult.IsValid);
            if (isValid == false)
            {
                var message = validationResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.NameLengthIsInvalid, 1, 50), message);
            }
        }

        [Test]
        public void Name_uniqueness_during_add_validation_works()
        {
            const string name = "ASD";
            BonusRepository.Templates.Add(new Template { Info = new TemplateInfo { Name = name, Brand = _template.Info.Brand } });

            _info.Name = name;
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotUnique, message);
        }

        [Test]
        public void Name_uniqueness_validation_works_during_edit()
        {
            var id = Guid.NewGuid();
            const string name = "ASD";
            BonusRepository.Templates.Add(new Template { Id = id, Info = new TemplateInfo { Name = name, Brand = _template.Info.Brand } });
            BonusRepository.Templates.Add(new Template { Id = Guid.NewGuid(), Info = new TemplateInfo { Name = name, Brand = _template.Info.Brand } });

            _model.Id = id;
            _info.Name = name;
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.That(validationResult.Errors.Any(e => e.ErrorMessage == ValidatorMessages.NameIsNotUnique));
        }

        [Test]
        public void Name_uniqueness_is_persisted_per_brand()
        {
            const string name = "ASD";
            BonusRepository.Templates.Add(new Template { Info = new TemplateInfo { Name = name, Brand = new Brand { Id = Guid.NewGuid() } } });

            _info.Name = name;
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(true, validationResult.IsValid);
        }

        [Test]
        public void Name_uniqueness_ignores_deleted_templates()
        {
            const string name = "ASD";
            BonusRepository.Templates.Add(new Template
            {
                Info = new TemplateInfo { Name = name, Brand = _template.Info.Brand },
                Status = TemplateStatus.Deleted
            });

            _info.Name = name;
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            validationResult.IsValid.Should().BeTrue();
        }

        [Test]
        public void Transaction_amount_limit_cant_be_negative()
        {
            _rules.RewardTiers.Single().BonusTiers.Single().MaxAmount = -1;
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateTransactionAmountLimitIsNegative, message);
        }

        [Test]
        public void Player_count_limit_cant_be_negative()
        {
            _model.Availability = new CreateUpdateTemplateAvailability { RedemptionsLimit = -1 };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplatePlayerRedemptionLimitIsNegative, message);
        }

        [Test]
        public void Reward_amount_limit_cant_be_negative()
        {
            _rules.RewardTiers.Single().RewardAmountLimit = -1;
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRewardLimitIsNegative, message);
        }

        [TestCase(200, true)]
        [TestCase(201, false)]
        public void Remarks_length_validation_works(int length, bool isValid)
        {
            _info.Description = TestDataGenerator.GetRandomString(length);
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(isValid, validationResult.IsValid);
            if (isValid == false)
            {
                var message = validationResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200), message);
            }
        }

        [Test]
        public void Unknown_brandId_isnt_valid()
        {
            var model = new CreateUpdateTemplate
            {
                Id = Guid.Empty,
                Info = new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    BrandId = Guid.NewGuid(),
                    WalletTemplateId = Guid.NewGuid(),
                    TemplateType = BonusType.FirstDeposit,
                    Mode = IssuanceMode.Automatic
                }
            };

            var validationResult = BonusQueries.GetValidationResult(model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBrandDoesNotExist, message);
        }

        [Test]
        public void Currency_not_specified_validation_works()
        {
            _rules.RewardTiers = new List<CreateUpdateRewardTier>();
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateCurrenciesAreEmpty, message);
        }

        [Test]
        public void Wagering_condition_cant_be_negative()
        {
            _model.Wagering = new CreateUpdateTemplateWagering
            {
                Multiplier = -1
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringConditionIsNegative, message);
        }

        [Test]
        public void Wagering_threshold_cant_be_negative()
        {
            _model.Wagering = new CreateUpdateTemplateWagering
            {
                Threshold = -1
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringThresholdIsNegative, message);
        }

        [Test]
        public void Wagering_contributions_cant_be_negative()
        {
            var gameId = Guid.NewGuid();
            BonusRepository.Games.Add(new Game { Id = gameId });
            _model.Wagering = new CreateUpdateTemplateWagering
            {
                GameContributions = new List<CreateUpdateGameContribution>
                {
                    new CreateUpdateGameContribution
                    {
                        GameId = gameId,
                        Contribution = -1
                    }
                }
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateOneOfGameContributionsIsNegative, message);
        }

        [Test]
        public void Unknown_gameId_in_wagering_contribution_isnt_valid()
        {
            var gameId = Guid.NewGuid();
            BonusRepository.Games.Add(new Game());
            BonusRepository.Games.Add(new Game { Id = gameId });
            _model.Wagering = new CreateUpdateTemplateWagering
            {
                GameContributions = new List<CreateUpdateGameContribution>
                {
                    new CreateUpdateGameContribution
                    {
                        GameId = gameId,
                        Contribution = 0
                    },
                    new CreateUpdateGameContribution
                    {
                        GameId = new Guid(),
                        Contribution = 1
                    }
                }
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateOneOfGameContributionsPointsToInvalidGame, message);
        }

        [Test]
        public void After_wager_template_requires_wagering_multiplier()
        {
            _model.Wagering = new CreateUpdateTemplateWagering { HasWagering = true, Multiplier = 0 };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringConditionIsZeroOrLess, message);
        }

        [TestCase(BonusType.ReloadDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, ExpectedResult = false)]
        public bool Wagering_method_validation_works(BonusType bonusType)
        {
            _template.Info.TemplateType = bonusType;
            _template.Rules.FundInWallets = _template.Info.Brand.WalletTemplates.Select(w => new BonusFundInWallet { WalletId = w.Id }).ToList();

            _model.Wagering = new CreateUpdateTemplateWagering
            {
                Method = WageringMethod.BonusAndTransferAmount
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            if (validationResult.IsValid == false)
            {
                Assert.True(
                    validationResult.Errors.Any(
                        error =>
                            error.ErrorMessage == ValidatorMessages.TemplateWageringMethodIsNotSupportedByBonusTrigger));
            }

            return validationResult.IsValid;
        }

        [TestCase(BonusType.FirstDeposit, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, BonusRewardType.Percentage, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, BonusRewardType.TieredPercentage, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.Percentage, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.TieredPercentage, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.Amount, ExpectedResult = false)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.Percentage, ExpectedResult = false)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.TieredPercentage, ExpectedResult = false)]
        [TestCase(BonusType.FundIn, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, BonusRewardType.Percentage, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, BonusRewardType.TieredPercentage, ExpectedResult = true)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.Percentage, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.TieredAmount, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.TieredPercentage, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.Amount, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.Percentage, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.TieredPercentage, ExpectedResult = false)]
        public bool Reward_type_validation_works(BonusType bonusType, BonusRewardType rewardType)
        {
            _template.Info.TemplateType = bonusType;

            _rules.RewardType = rewardType;
            _rules.FundInWallets = _template.Info.Brand.WalletTemplates.Select(w => w.Id).ToList();
            if (rewardType == BonusRewardType.TieredAmount || rewardType == BonusRewardType.TieredPercentage)
            {
                if (bonusType == BonusType.HighDeposit)
                {
                    _rules.RewardTiers.Single().BonusTiers = new List<CreateUpdateTemplateTier>
                    {
                        new CreateUpdateTemplateTier {Reward = 1, From = 1, NotificationPercentThreshold = 1}
                    };
                }
                else
                {
                    _rules.RewardTiers.Single().BonusTiers = new List<CreateUpdateTemplateTier>
                    {
                        new CreateUpdateTemplateTier {From = 1, Reward = 100}
                    };
                }
            }
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            if (validationResult.IsValid == false)
            {
                Assert.True(
                    validationResult.Errors.Any(
                        error => error.ErrorMessage == ValidatorMessages.TemplateRewardTypeIsNotSupported));
            }

            return validationResult.IsValid;
        }

        [Test]
        public void At_least_one_bonus_tier_is_required_for_tiered_template()
        {
            _rules.RewardType = BonusRewardType.TieredAmount;
            _rules.RewardTiers.Single().BonusTiers = new List<CreateUpdateTemplateTier>();
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTierAtLeastOneIsRequired, message);
        }

        [Test]
        public void Bonus_tiers_overlap_validation_works()
        {
            _rules.RewardType = BonusRewardType.TieredAmount;
            _rules.RewardTiers.Single().BonusTiers = new List<CreateUpdateTemplateTier>
            {
                new CreateUpdateTemplateTier {From = 10, Reward = 10},
                new CreateUpdateTemplateTier {From = 5, Reward = 20},
                new CreateUpdateTemplateTier {From = 100, Reward = 30}
            };
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTiersOverlap, message);
        }

        [Test]
        public void At_least_one_high_deposit_tier_is_required_validation_works()
        {
            _template.Info.TemplateType = BonusType.HighDeposit;

            _rules.RewardType = BonusRewardType.TieredAmount;
            _rules.RewardTiers.Single().BonusTiers = new List<CreateUpdateTemplateTier>();
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTierAtLeastOneIsRequired, message);
        }

        [Test]
        public void Duplicate_high_deposit_tiers_validation_works()
        {
            _template.Info.TemplateType = BonusType.HighDeposit;
            _rules.RewardType = BonusRewardType.TieredAmount;
            _rules.RewardTiers.Single().BonusTiers = new List<CreateUpdateTemplateTier>
            {
                new CreateUpdateTemplateTier {From = 100, Reward = 10},
                new CreateUpdateTemplateTier {From = 200, Reward = 10},
                new CreateUpdateTemplateTier {From = 200, Reward = 10}
            };
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateDuplicateHighDepositTiers, message);
        }

        [Test]
        public void Parent_bonus_is_absent_validation_works()
        {
            _model.Availability = new CreateUpdateTemplateAvailability { ParentBonusId = Guid.NewGuid() };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateParentBonusDoesNotExist, message);
        }

        [Test]
        public void Player_registration_date_range_validation_works()
        {
            _model.Availability = new CreateUpdateTemplateAvailability
            {
                PlayerRegistrationDateFrom = DateTimeOffset.Now.AddDays(-3).Date,
                PlayerRegistrationDateTo = DateTimeOffset.Now.AddDays(-4).Date
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplatePlayerRegistrationDateRangeIsInvalid, message);
        }

        [Test]
        public void Reward_amount_should_be_greater_than_zero()
        {
            _rules.RewardTiers.Single().BonusTiers.Single().Reward = 0;
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRewardValueIsInvalid, message);
        }

        [Test]
        public void Exclude_bonuses_contain_parent_bonus_validation_works()
        {
            var parentId = Guid.NewGuid();
            _model.Availability = new CreateUpdateTemplateAvailability
            {
                ParentBonusId = parentId,
                ExcludeBonuses = new List<Guid>
                {
                    parentId
                }
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.True(validationResult.Errors.Any(a => a.ErrorMessage == ValidatorMessages.TemplateBonusExcludesContainsParentBonus));
        }

        [Test]
        public void Fund_in_no_wallets_validation_works()
        {
            _template.Info.TemplateType = BonusType.FundIn;
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateNoFundInWallets, message);
        }

        [Test]
        public void Fund_in_wallets_should_be_brand_related()
        {
            _template.Info.TemplateType = BonusType.FundIn;
            _rules.FundInWallets = new List<Guid> { Guid.NewGuid() };
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateFundInWalletsAreInvalid, message);
        }

        [Test]
        public void Player_redemption_limit_cant_be_negative()
        {
            _model.Availability = new CreateUpdateTemplateAvailability { PlayerRedemptionsLimit = -1 };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplatePlayerRedemptionsIsNegative, message);
        }

        [Test]
        public void Can_not_update_non_existing_template()
        {
            _model.Id = Guid.NewGuid();

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(1, validationResult.Errors.Count(e => e.ErrorMessage == ValidatorMessages.TemplateDoesNotExist));
        }

        [Test]
        public void Can_not_update_not_current_version_template()
        {
            _model.Info = _info;
            _template.Version = 1000;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(1, validationResult.Errors.Count(e => e.ErrorMessage == ValidatorMessages.TemplateVersionIsNotCurrent));
        }

        [Test]
        public void Can_not_edit_template_if_there_are_active_bonuses_using_it()
        {
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Template = _template, IsActive = true });
            var model = new CreateUpdateTemplate
            {
                Id = _template.Id,
                Version = _template.Version,
                Notification = new CreateUpdateTemplateNotification()
            };

            var validationResult = BonusQueries.GetValidationResult(model);
            Assert.AreEqual(1, validationResult.Errors.Count(e => e.ErrorMessage == ValidatorMessages.AllBonusesShouldBeInactive));
        }

        [Test]
        public void Template_should_have_receiving_wallet()
        {
            _info.WalletTemplateId = Guid.Empty;
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateReceivingWalletIsNotSpecified, message);
        }

        [Test]
        public void Template_should_have_receiving_wallet_that_is_brand_related()
        {
            _info.WalletTemplateId = Guid.NewGuid();
            _model.Info = _info;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateReceivingWalletIsInvalid, message);
        }

        [Test]
        public void Template_with_no_rollover_can_not_be_issued_after_wagering()
        {
            _model.Wagering = new CreateUpdateTemplateWagering
            {
                HasWagering = false,
                IsAfterWager = true
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringIsAfterWagerIsNotApplicable, message);
        }

        [TestCase(BonusType.ReloadDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, ExpectedResult = false)]
        public bool IssuanceMode_with_template_type_compartability_validation_works(BonusType bonusType)
        {
            var model = new CreateUpdateTemplate
            {
                Info = _info
            };
            model.Info.TemplateType = bonusType;
            model.Info.Mode = IssuanceMode.AutomaticWithCode;

            var validationResult = BonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
            {
                var message = validationResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(ValidatorMessages.TemplateModeIsIncorrect, message);
            }

            return validationResult.IsValid;
        }

        [Test]
        public void Template_with_currencies_of_bonus_tiers_that_are_not_brand_related_is_invalid()
        {
            _rules.RewardTiers.Add(new CreateUpdateRewardTier
            {
                CurrencyCode = "InvalidCode",
                BonusTiers = new List<CreateUpdateTemplateTier>
                            {
                                new CreateUpdateTemplateTier {Reward = 25}
                            }
            });
            _model.Rules = _rules;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRewardCurrenciesAreInvalid, message);
        }

        [Test]
        public void Template_VIPs_are_not_brand_related_validation_works()
        {
            _model.Availability = new CreateUpdateTemplateAvailability
            {
                VipLevels = new List<string> { TestDataGenerator.GetRandomString() }
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateVipsAreInvalid, message);
        }

        [Test]
        public void Template_RiskLevels_are_not_brand_related_validation_works()
        {
            _model.Availability = new CreateUpdateTemplateAvailability
            {
                ExcludeRiskLevels = new List<Guid> { Guid.NewGuid() }
            };

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRiskLevelsAreInvalid, message);
        }
    }
}