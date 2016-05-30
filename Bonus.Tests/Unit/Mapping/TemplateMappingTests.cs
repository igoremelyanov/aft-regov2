using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Mapping
{
    internal class TemplateMappingTests : UnitTestBase
    {
        private CreateUpdateTemplate _model;
        private BonusMapper _bonusMapper;
        private Brand _brand;
        private BonusManagementCommands _bonusManagementCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            _bonusMapper = Container.Resolve<BonusMapper>();

            _brand = BonusRepository.Brands.First();
            _model = new CreateUpdateTemplate
            {
                Id = Guid.Empty,
                Info = new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit,
                    BrandId = _brand.Id,
                    WalletTemplateId = _brand.WalletTemplates.First().Id,
                    Mode = IssuanceMode.Automatic
                }
            };
        }

        [Test]
        public void Mapper_assigns_brand_to_new_template()
        {
            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Info.Brand.Id.Should().Be(_brand.Id);
        }

        [Test]
        public void Mapper_assigns_brand_to_existing_template()
        {
            var identifier = _bonusManagementCommands.AddUpdateTemplate(_model);

            _model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Availability = new CreateUpdateTemplateAvailability()
            };

            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Info.Brand.Id.Should().Be(_brand.Id);
        }

        [Test]
        public void Wagering_game_contribution_percentage_turns_into_decimal_representation()
        {
            var identifier = _bonusManagementCommands.AddUpdateTemplate(_model);

            _model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Wagering = new CreateUpdateTemplateWagering
                {
                    HasWagering = true,
                    Multiplier = 1,
                    GameContributions = new List<CreateUpdateGameContribution> { new CreateUpdateGameContribution { Contribution = 45 } }
                }
            };

            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Wagering.GameContributions.Single().Contribution.Should().Be(0.45m);
        }

        [Test]
        public void When_mapping_new_template_only_Info_part_is_preserved()
        {
            _model.Availability = new CreateUpdateTemplateAvailability();
            _model.Rules = new CreateUpdateTemplateRules();
            _model.Wagering = new CreateUpdateTemplateWagering();
            _model.Notification = new CreateUpdateTemplateNotification();

            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Info.Should().NotBeNull();
            template.Availability.Should().BeNull();
            template.Rules.Should().BeNull();
            template.Wagering.Should().BeNull();
            template.Notification.Should().BeNull();
        }

        [Test]
        public void HighDeposit_bonus_tier_notification_percenatage_is_converted_to_decimal()
        {
            _model.Info.TemplateType = BonusType.HighDeposit;
            var identifier = _bonusManagementCommands.AddUpdateTemplate(_model);

            _model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Rules = new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier{BonusTiers = new List<CreateUpdateTemplateTier>{new CreateUpdateTemplateTier{NotificationPercentThreshold = 44}}}
                    }
                }
            };

            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Rules.RewardTiers.Single()
                .BonusTiers.Single()
                .As<HighDepositTier>()
                .NotificationPercentThreshold.Should()
                .Be(0.44m);
        }

        [TestCase(BonusRewardType.Percentage, TestName = "Percentage reward is converted to decimal")]
        [TestCase(BonusRewardType.TieredPercentage, TestName = "Tiered percentage reward is converted to decimal")]
        public void Reward_is_converted_to_decimal(BonusRewardType type)
        {
            var identifier = _bonusManagementCommands.AddUpdateTemplate(_model);

            _model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Rules = new CreateUpdateTemplateRules
                {
                    RewardType = type,
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier{BonusTiers = new List<CreateUpdateTemplateTier>{new CreateUpdateTemplateTier{Reward = 69}}}
                    }
                }
            };

            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Rules.RewardTiers.Single()
                .BonusTiers.Single()
                .Reward.Should()
                .Be(0.69m);
        }

        [Test]
        public void Player_registration_dates_are_mapped()
        {
            var identifier = _bonusManagementCommands.AddUpdateTemplate(_model);

            _model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Availability = new CreateUpdateTemplateAvailability
                {
                    PlayerRegistrationDateFrom = DateTimeOffset.Now.Date,
                    PlayerRegistrationDateTo = DateTimeOffset.Now.AddDays(1).Date,
                }
            };

            var template = _bonusMapper.MapModelToTemplate(_model);

            template.Availability.PlayerRegistrationDateFrom.Value.DateTime.Should()
                .Be(_model.Availability.PlayerRegistrationDateFrom.Value);

            template.Availability.PlayerRegistrationDateTo.Value.DateTime.Should()
                .Be(_model.Availability.PlayerRegistrationDateTo.Value);
        }
    }
}
