using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;
using Player = AFT.RegoV2.Bonus.Core.Data.Player;

namespace AFT.RegoV2.Bonus.Tests.Integration.Api
{
    internal class PlayerControllerTests : ApiIntegrationTestBase
    {
        private Player _player;

        public override void BeforeEach()
        {
            _player = CreatePlayer();
            LogInApi(_player.Id);
        }

        [Test]
        public async void Can_get_player_wagering_balance()
        {
            var info = CreateTemplateInfo(BonusType.FirstDeposit, IssuanceMode.AutomaticWithCode);
            var wagering = new CreateUpdateTemplateWagering
            {
                HasWagering = true,
                Multiplier = 3,
                Method = WageringMethod.Bonus
            };
            var template = CreateTemplate(info: info, wagering: wagering);
            var bonus = CreateBonus(template);

            MakeDeposit(_player.Id, bonusCode: bonus.Code);
            PlaceAndLoseBet(50, _player.Id);

            var result = await ApiProxy.GetWageringBalancesAsync(_player.Id);

            result.Completed.Should().Be(50);
            result.Remaining.Should().Be(31);
            result.Requirement.Should().Be(81);
        }

        [Test]
        public async void Can_get_player_balance()
        {
            var info = CreateTemplateInfo(BonusType.FirstDeposit, IssuanceMode.AutomaticWithCode);
            var wagering = new CreateUpdateTemplateWagering
            {
                HasWagering = true,
                Multiplier = 3,
                Method = WageringMethod.Bonus
            };
            var template = CreateTemplate(info: info, wagering: wagering);
            var bonus = CreateBonus(template);

            MakeDeposit(_player.Id, bonusCode: bonus.Code);
            PlaceAndLoseBet(50, _player.Id);

            var result = await ApiProxy.GetPlayerBalanceAsync(_player.Id);

            result.Main.Should().Be(150);
            result.Bonus.Should().Be(27);
        }

        [Test]
        public async void Can_get_claimable_redemptions()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            MakeDeposit(_player.Id, bonusId: bonus.Id);

            var result = await ApiProxy.GetClaimableRedemptionsAsync(_player.Id);

            result.Count.Should().Be(1);
            result.First().Bonus.Name.Should().Be(bonus.Name);
        }

        [Test, Ignore]
        public async void Can_get_deposit_qualified_bonuses()
        {
            var bonusWithCode = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            var bonusManual = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            var result = await ApiProxy.GetDepositQualifiedBonusesAsync(_player.Id);

            result.Should().NotBeEmpty();
            result.Any(b => b.Name == bonusManual.Name).Should().BeTrue();
            result.Any(b => b.Name == bonusWithCode.Name).Should().BeTrue();
        }

        [Test]
        public async void Can_get_visible_deposit_qualified_bonuses()
        {
            var bonusWithCode = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            var bonusManual = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            var result = await ApiProxy.GetVisibleDepositQualifiedBonuses(_player.Id);

            result.Should().NotBeEmpty();
            result.Any(b => b.Name == bonusManual.Name).Should().BeTrue();
            result.Any(b => b.Name == bonusWithCode.Name).Should().BeFalse();
        }

        [Test]
        public async void Can_get_deposit_qualified_bonus_by_code()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            var result = await ApiProxy.GetDepositQualifiedBonusByCodeAsync(_player.Id, bonus.Code, 0);

            result.Name.Should().Be(bonus.Name);
        }

        [Test]
        public async void Can_get_validation_result()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            await ApiProxy.GetDepositBonusApplicationValidationAsync(_player.Id, bonus.Code, 100);
        }

        [Test]
        public async void Can_claim_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            MakeDeposit(_player.Id, bonusId: bonus.Id);

            var result = await ApiProxy.GetClaimableRedemptionsAsync(_player.Id);
            await ApiProxy.ClaimBonusRedemptionAsync(new ClaimBonusRedemption
            {
                PlayerId = _player.Id,
                RedemptionId = result.First().Id
            });
        }

        [Test, Ignore("KB: wallet templates are not available for R1 to test the scenario")]
        public async void Can_apply_for_fund_in_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            MakeDeposit(_player.Id);

            await ApiProxy.ApplyForBonusAsync(new FundInBonusApplication
            {
                PlayerId = _player.Id,
                BonusId = bonus.Id,
                Amount = 100
            });
        }

        [Test]
        public async void Can_apply_for_deposit_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            var result = await ApiProxy.ApplyForBonusAsync(new DepositBonusApplication
            {
                PlayerId = _player.Id,
                BonusId = bonus.Id,
                Amount = 100,
                DepositId = Guid.NewGuid()
            });

            result.Should().NotBeEmpty();
        }

        [Test]
        public async void Can_get_bonus_redemptions_with_active_wagering_requirement()
        {
            var info = CreateTemplateInfo(BonusType.FirstDeposit, IssuanceMode.AutomaticWithCode);
            var wagering = new CreateUpdateTemplateWagering
            {
                HasWagering = true,
                Multiplier = 3,
                Method = WageringMethod.Bonus
            };
            var template = CreateTemplate(info: info, wagering: wagering);
            var bonus = CreateBonus(template);

            MakeDeposit(_player.Id, bonusCode: bonus.Code);

            var result = await ApiProxy.GetBonusesWithIncompleteWageringAsync(_player.Id);

            result.Should().NotBeEmpty();
            result.Single().Bonus.Name.Should().Be(bonus.Name);
        }

        [Test]
        public async void Can_get_completed_bonus_redemptions()
        {
            var manualBonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            var info = CreateTemplateInfo(BonusType.ReloadDeposit, IssuanceMode.AutomaticWithCode);
            var reloadTemplate = CreateTemplate(info);
            var reloadBonus = CreateBonus(reloadTemplate);

            MakeDeposit(_player.Id, bonusId: manualBonus.Id);
            MakeDeposit(_player.Id, bonusCode: reloadBonus.Code);

            manualBonus.DurationEnd = manualBonus.DurationEnd.AddDays(-2);
            BonusRepository.SaveChanges();

            var result = await ApiProxy.GetCompletedBonusesAsync(_player.Id);

            result.Should().NotBeEmpty();
            result.Count.Should().Be(2);
        }
    }
}