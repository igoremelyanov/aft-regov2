using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Events.Player;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Types
{
    internal class MobilePlusEmailVerificationTests : UnitTestBase
    {
        [Test]
        public void Can_activate_mobile_plus_email_verification_bonus()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.MobilePlusEmailVerification;

            var player = CreatePlayer();
            var bonusRedemption = player.Wallets.First().BonusesRedeemed.First();

            Assert.AreEqual(ActivationStatus.Pending, bonusRedemption.ActivationState);
            Assert.AreEqual(27, bonusRedemption.Amount);

            VerifyPlayerContact(player.Id, ContactType.Mobile);
            VerifyPlayerContact(player.Id, ContactType.Email);

            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(27, bonusRedemption.Amount);
        }

        [TestCase(ContactType.Mobile)]
        [TestCase(ContactType.Email)]
        public void Cannot_redeem_mobile_plus_email_verification_bonus_without_both_verified(ContactType contactType)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.MobilePlusEmailVerification;

            var player = CreatePlayer();
            var bonusRedemption = player.Wallets.First().BonusesRedeemed.First();

            VerifyPlayerContact(player.Id, contactType);

            Assert.AreEqual(ActivationStatus.Pending, bonusRedemption.ActivationState);
        }
    }
}