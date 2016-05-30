using System.Linq;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class ReceivingWalletTests : UnitTestBase
    {
        [Test]
        public void Bonus_is_issued_to_the_correct_player_wallet()
        {
            var player = BonusRepository.Players.Single(p => p.Id == PlayerId);
            var wallet = player.Wallets.Single(w => w.Template.IsMain == false);
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.WalletTemplateId = wallet.Template.Id;

            MakeDeposit(PlayerId);

            wallet.NonTransferableBonus.Should().Be(27);
        }
    }
}