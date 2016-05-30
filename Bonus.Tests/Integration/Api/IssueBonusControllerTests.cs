using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Shared.ApiDataFiltering;
using FluentAssertions;
using NUnit.Framework;
using TransactionType = AFT.RegoV2.Bonus.Core.Models.Enums.TransactionType;

namespace AFT.RegoV2.Bonus.Tests.Integration.Api
{
    public class IssueBonusControllerTests : ApiIntegrationTestBase
    {
        [Test]
        public async void Can_get_qualified_bonuses()
        {
            var player = CreatePlayer();
            MakeDeposit(player.Id);
            CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);

            var request = new FilteredDataRequest
            {
                PageIndex = 1,
                RowCount = 20,
                SortSord = "asc",
                SortColumn = "Name",
                TopRecords = 20,
                Filters = new Filter[] {}
            };
            var result = await ApiProxy.GetFilteredIssueBonusesAsync(new PlayerFilteredDataRequest
            {
                DataRequest = request,
                PlayerId = player.Id
            });

            result.Should().NotBeNull();
            result.Rows.Count.Should().BeGreaterOrEqualTo(1).And.BeLessOrEqualTo(20);
        }

        [Test]
        public async void Can_get_transactions()
        {
            var player = CreatePlayer();
            MakeDeposit(player.Id);
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);

            var result = await ApiProxy.GetIssueBonusTransactionsAsync(player.Id, bonus.Id);

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Test]
        public async void Can_issue_bonus_by_CS()
        {
            var player = CreatePlayer();
            MakeDeposit(player.Id);
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);
            var transaction = player.Wallets.SelectMany(w => w.Transactions).Single(t => t.Type == TransactionType.Deposit);

            var result = await ApiProxy.IssueBonusAsync(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = player.Id,
                TransactionId = transaction.Id
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }
    }
}
