using System;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Shared.ApiDataFiltering;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Integration.Api
{
    public class BonusHistoryControllerTests : ApiIntegrationTestBase
    {
        [Test]
        public async void Can_get_filtered_bonus_redemptions()
        {
            var player = CreatePlayer();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            MakeDeposit(player.Id, bonusCode: bonus.Code);

            var request = new FilteredDataRequest
            {
                PageIndex = 1,
                RowCount = 20,
                SortSord = "asc",
                SortColumn = "Bonus.Name",
                TopRecords = 20,
                Filters = null
            };
            var result = await ApiProxy.GetFilteredBonusRedemptionAsync(new PlayerFilteredDataRequest
            {
                PlayerId = player.Id,
                DataRequest = request
            });

            result.Should().NotBeNull();
            result.Rows.Count.Should().BeGreaterOrEqualTo(1).And.BeLessOrEqualTo(20);
        }

        [Test]
        public async void Can_get_bonus_redemption_view_data()
        {
            var player = CreatePlayer();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            MakeDeposit(player.Id, bonusCode: bonus.Code);
            var redemption = player.Wallets.SelectMany(w => w.BonusesRedeemed).Single(br => br.Bonus.Id == bonus.Id);

            var result = await ApiProxy.GetBonusRedemptionAsync(player.Id, redemption.Id);

            result.Should().NotBeNull();
            result.Bonus.Name.Should().Be(bonus.Name);
        }

        [Test]
        public async void Can_cancel_bonus_redemption()
        {
            var player = CreatePlayer();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 2;
            BonusRepository.SaveChanges();

            MakeDeposit(player.Id, bonusCode: bonus.Code);
            var redemption = player.Wallets.SelectMany(w => w.BonusesRedeemed).Single(br => br.Bonus.Id == bonus.Id);

            var result = await ApiProxy.CancelBonusRedemptionAsync(new CancelBonusRedemption
            {
                PlayerId = player.Id,
                RedemptionId = redemption.Id
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void Can_get_filtered_bonus_redemption_events()
        {
            CreatePlayer();

            var request = new FilteredDataRequest
            {
                PageIndex = 1,
                RowCount = 20,
                SortSord = "asc",
                SortColumn = "Created",
                TopRecords = 20
            };
            var result = ApiProxy.GetFilteredBonusRedemptionEventsAsync(new RedemptionFilteredDataRequest
            {
                DataRequest = request,
                BonusRedemptionId = Guid.Empty
            });

            result.Should().NotBeNull();
        }
    }
}