using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit
{
    internal class ManagementTests : UnitTestBase
    {
        [TestCase(true, Description = "Activating active bonus does nothing")]
        [TestCase(false, Description = "Deactivating inactive bonus does nothing")]
        public void Update_status_returns_if_input_status_matches_status_in_db(bool isActive)
        {
            var id = Guid.NewGuid();
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = id, Template = new Template(), IsActive = isActive });

            Container.Resolve<BonusManagementCommands>().ChangeBonusStatus(new ToggleBonusStatus { Id = id, IsActive = isActive });

            Assert.AreEqual(1, BonusRepository.Bonuses.Count(b => b.Id == id));
        }

        [Test]
        public void Template_update_does_not_affect_already_redemed_bonuses()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().MaxAmount = 10;
            BonusRepository.Templates.Add(bonus.Template);

            var depositId = SubmitDeposit(PlayerId, 200, bonus.Code);

            var updatedBonus = CreateFirstDepositBonus();
            updatedBonus.Template.Id = bonus.Template.Id;
            updatedBonus.Template.Version = 1;
            updatedBonus.Version = 1;
            updatedBonus.Template.Rules.RewardTiers.Single().Tiers.Single().MaxAmount = 10;
            BonusRepository.Templates.Add(updatedBonus.Template);

            ApproveDeposit(depositId, PlayerId, 200);
            Assert.AreEqual(10, BonusRedemptions.Single().Amount);
        }
    }
}