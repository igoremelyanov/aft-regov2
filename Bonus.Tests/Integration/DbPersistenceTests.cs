using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Integration
{
    internal class DbPersistenceTests : IntegrationTestBase
    {
        private BonusQueries _bonusQueries;
        private BonusManagementCommands _bonusManagementCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonusQueries = Container.Resolve<BonusQueries>();
            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
        }

        [Test]
        public void Updated_bonus_is_correctly_saved_to_DB()
        {
            var template = CreateFirstDepositTemplate(mode: IssuanceMode.AutomaticWithCode);
            var baseBonus = CreateFirstDepositBonus(isActive: false, mode: IssuanceMode.AutomaticWithCode);

            var model = new CreateUpdateBonus
            {
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                Description = TestDataGenerator.GetRandomString(),
                TemplateId = template.Id,
                Id = baseBonus.Id,
                Version = baseBonus.Version,
                DurationType = DurationType.None,
                ActiveFrom = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId).Date,
                ActiveTo = DateTimeOffset.Now.AddDays(1).ToBrandOffset(template.Info.Brand.TimezoneId).Date
            };

            var bonusId = _bonusManagementCommands.AddUpdateBonus(model);

            var updatedBonus = BonusRepository.GetCurrentVersionBonuses().Single(a => a.Id == bonusId);

            Assert.AreEqual(1, updatedBonus.Version);
            Assert.AreEqual(model.Code, updatedBonus.Code);
        }

        [Test]
        public void Bonus_deactivation_is_saved_to_DB()
        {
            var bonus = CreateFirstDepositBonus();
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus { Id = bonus.Id });

            var updatedBonus = BonusRepository.GetCurrentVersionBonuses().Single(a => a.Id == bonus.Id);

            Assert.False(updatedBonus.IsActive);
        }

        [Test]
        public void Statistic_persist_between_bonus_versions()
        {
            var bonus = CreateFirstDepositBonus();

            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus { Id = bonus.Id });
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus { Id = bonus.Id });

            var statisticIds = BonusRepository.Bonuses.Where(a => a.Id == bonus.Id).Select(s => s.Statistic.Id).ToList();

            //assert that all ids are the same
            Assert.True(statisticIds.All(id => id == statisticIds.First()));
        }

        [Test]
        public void Bonus_activation_do_not_create_new_statistic_record()
        {
            const decimal depositAmount = 100;
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            var player = CreatePlayer();

            MakeDeposit(player.Id, depositAmount, bonus.Code);
            var statisticIds = BonusRepository.Bonuses.Where(a => a.Id == bonus.Id).Select(s => s.Statistic.Id).ToList();

            //assert that all ids are the same
            Assert.True(statisticIds.All(id => id == statisticIds.First()));

            //deactivate bonus
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus { Id = bonus.Id });
        }

        [Test]
        public void Add_template_saves_all_data_to_DB()
        {
            var template = CreateFirstDepositTemplate();
            var templateFromDb = _bonusQueries.GetCurrentVersionTemplates().Single(t => t.Id == template.Id);

            Assert.NotNull(templateFromDb.Info);
            Assert.NotNull(templateFromDb.Availability);
            Assert.NotNull(templateFromDb.Rules);
            Assert.NotNull(templateFromDb.Wagering);
            Assert.NotNull(templateFromDb.Notification);
            Assert.AreEqual(TemplateStatus.Complete, templateFromDb.Status);
        }

        [Test]
        public void Updated_template_is_correctly_saved_to_DB()
        {
            var baseTemplate = CreateFirstDepositTemplate();

            var model = new CreateUpdateTemplate
            {
                Id = baseTemplate.Id,
                Version = baseTemplate.Version,
                Wagering = new CreateUpdateTemplateWagering
                {
                    Threshold = 1000,
                    GameContributions = 
                    {
                        new CreateUpdateGameContribution
                        {
                            GameId = new Guid("B641B4E9-CA08-4443-8FD3-8D1A43727C3E"),
                            Contribution = 4
                        }
                    }
                }
            };
            _bonusManagementCommands.AddUpdateTemplate(model);
            var updatedTemplate = _bonusQueries.GetCurrentVersionTemplates().Single(a => a.Id == baseTemplate.Id);

            Assert.AreEqual(1, updatedTemplate.Version);
            Assert.AreEqual(1000, updatedTemplate.Wagering.Threshold);
            Assert.AreEqual(0.04, updatedTemplate.Wagering.GameContributions.First().Contribution);
        }

        [Test]
        public void Can_delete_template()
        {
            var template = CreateFirstDepositTemplate();
            _bonusManagementCommands.DeleteTemplate(new DeleteTemplate { TemplateId = template.Id });

            template = _bonusQueries.GetCurrentVersionTemplates().SingleOrDefault(t => t.Id == template.Id);
            Assert.Null(template);
        }

        [Test]
        public void Vips_count_does_not_change_between_template_edits()
        {
            var brandId = new Guid("00000000-0000-0000-0000-000000000138");
            var brand = BonusRepository.Brands.Include(x => x.WalletTemplates).First(x => x.Id == brandId);

            var model = new CreateUpdateTemplate
            {
                Info = new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit,
                    BrandId = brand.Id,
                    WalletTemplateId = brand.WalletTemplates.First().Id
                }
            };
            var identifier = _bonusManagementCommands.AddUpdateTemplate(model);

            model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Availability = new CreateUpdateTemplateAvailability { VipLevels = new List<string> { "S" } }
            };

            identifier = _bonusManagementCommands.AddUpdateTemplate(model);

            var templateFromDb = BonusRepository.Templates.Single(t => t.Id == identifier.Id && t.Version == identifier.Version);
            Assert.AreEqual(1, templateFromDb.Availability.VipLevels.Count);

            model = new CreateUpdateTemplate
            {
                Id = identifier.Id,
                Version = identifier.Version,
                Rules = new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier> { new CreateUpdateTemplateTier { Reward = 25} }
                        }
                    },
                    RewardType = BonusRewardType.Amount
                }
            };
            identifier = _bonusManagementCommands.AddUpdateTemplate(model);

            templateFromDb = BonusRepository.Templates.Single(t => t.Id == identifier.Id && t.Version == identifier.Version);
            Assert.AreEqual(1, templateFromDb.Availability.VipLevels.Count);
        }

        [Test]
        public void Template_update_updates_bonus()
        {
            var initialBonus = CreateFirstDepositBonus(false);

            var model = new CreateUpdateTemplate
            {
                Id = initialBonus.Template.Id,
                Version = initialBonus.Template.Version,
                Rules = new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier> { new CreateUpdateTemplateTier { Reward = 25} }
                        }
                    },
                    RewardType = BonusRewardType.Percentage
                }
            };
            _bonusManagementCommands.AddUpdateTemplate(model);

            var updatedBonus = BonusRepository.GetCurrentVersionBonuses().Single(bonus => bonus.Id == initialBonus.Id);

            Assert.AreEqual(1, updatedBonus.Version);
            Assert.AreEqual(1, updatedBonus.Template.Version);
        }
    }
}