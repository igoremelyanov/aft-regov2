using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Integration.Api
{
    internal class AdminControllerTests: ApiIntegrationTestBase
    {
        [Test]
        public async void Can_get_qualified_deposit_bonuses()
        {
            var player = CreatePlayer();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            var result = await ApiProxy.GetDepositQualifiedBonusesByAdminAsync(player.Id);

            result.Should().NotBeEmpty();
            result.SingleOrDefault(b => b.Id == bonus.Id).Should().NotBeNull();
        }
    }
}
