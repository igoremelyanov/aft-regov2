using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Utils;
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
    internal class BonusValidationTests : UnitTestBase
    {
        private CreateUpdateBonus _model;
        private string _brandTimezoneId;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var brand = BonusRepository.Brands.First();
            var template = new Template
            {
                Status = TemplateStatus.Complete,
                Info = new TemplateInfo
                {
                    TemplateType = BonusType.ReloadDeposit,
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

            BonusRepository.Templates.Add(template);

            _brandTimezoneId = brand.TimezoneId;
            _model = new CreateUpdateBonus
            {
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                Description = TestDataGenerator.GetRandomString(),
                TemplateId = template.Id,
                ActiveFrom = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).Date,
                ActiveTo = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).AddDays(1).Date,
                DurationType = DurationType.None
            };
        }

        [Test]
        public void Activity_dates_should_form_range()
        {
            _model.ActiveFrom = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).AddDays(2).Date;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusActivityRangeIsInvalid, message);
        }

        [Test]
        public void Bonus_activeTo_forms_active_daterange_validation_works()
        {
            _model.ActiveFrom = DateTimeOffset.Now.AddDays(-2).ToBrandOffset(_brandTimezoneId).Date;
            _model.ActiveTo = DateTimeOffset.Now.AddDays(-1).ToBrandOffset(_brandTimezoneId).Date;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusActiveToIsInvalid, message);
        }

        [Test]
        public void Name_is_required_validation_works()
        {
            _model.Name = null;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotSpecified, message);
        }

        [TestCase(50, true)]
        [TestCase(51, false)]
        public void Name_length_validation_works(int length, bool isValid)
        {
            _model.Name = TestDataGenerator.GetRandomString(length);

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
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Name = name });

            _model.Name = name;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotUnique, message);
        }

        [Test]
        public void Name_uniqueness_during_edit_validation_works()
        {
            var id = Guid.NewGuid();
            const string name = "ASD";
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = id, Name = name });
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = Guid.NewGuid(), Name = name });

            _model.Id = id;
            _model.Name = name;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotUnique, message);
        }

        [Test]
        public void Description_is_required_validation_works()
        {
            _model.Description = null;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(false, validationResult.IsValid);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.DescriptionIsRequired, message);
        }

        [TestCase(0, false)]
        [TestCase(200, true)]
        [TestCase(201, false)]
        public void Description_length_validation_works(int length, bool isValid)
        {
            _model.Description = TestDataGenerator.GetRandomString(length);

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(isValid, validationResult.IsValid);
            if (isValid == false)
            {
                var message = validationResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200), message);
            }
        }

        [Test]
        public void Template_does_not_exist_validation_works()
        {
            _model.TemplateId = Guid.NewGuid();

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusTemplateIsNotAssigned, message);
        }

        [Test]
        public void Code_is_required_validation_works_for_AutomaticWithBonusCode_issuance_mode()
        {
            BonusRepository.Templates.Single(t => t.Id == _model.TemplateId).Info.Mode = IssuanceMode.AutomaticWithCode;
            _model.Code = null;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsNotSpecified, message);
        }

        [Test]
        public void Code_cant_contain_special_characters()
        {
            _model.Code = "Foo Bar$";

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsInvalid, message);
        }

        [TestCase(1, true)]
        [TestCase(20, true)]
        [TestCase(21, false)]
        public void Code_length_validation_works(int length, bool isValid)
        {
            _model.Code = TestDataGenerator.GetRandomString(length);

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(isValid, validationResult.IsValid);
            if (isValid == false)
            {
                var message = validationResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.BonusCodeLengthIsInvalid, 1, 20), message);
            }
        }

        [Test]
        public void Code_uniqueness_during_add_validation_works()
        {
            const string code = "ASD";
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Code = code });

            _model.Code = code;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsNotUnique, message);
        }

        [Test]
        public void Code_uniqueness_uses_current_version_bonuses_during_add_validation()
        {
            const string code = "ASD";
            var bonusId = Guid.NewGuid();
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = bonusId, Version = 0, Code = code });
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = bonusId, Version = 1, Code = "OtherCode" });

            _model.Code = code;

            var validationResult = BonusQueries.GetValidationResult(_model);
            validationResult.IsValid.Should().BeTrue();
        }

        [Test]
        public void Code_uniqueness_during_edit_validation_works()
        {
            var id = Guid.NewGuid();
            const string code = "ASD";
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = id, Code = code });
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = Guid.NewGuid(), Code = code });

            _model.Id = id;
            _model.Code = code;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsNotUnique, message);
        }

        [Test]
        public void Code_uniqueness_uses_current_version_bonuses_during_edit_validation()
        {
            var bonusToEditId = Guid.NewGuid();
            var otherBonusdId = Guid.NewGuid();
            const string code = "ASD";
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = bonusToEditId, Code = code });
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = otherBonusdId, Version = 0, Code = code });
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = otherBonusdId, Version = 1, Code = "OtherCode" });

            _model.Id = bonusToEditId;
            _model.Code = code;

            var validationResult = BonusQueries.GetValidationResult(_model);
            validationResult.IsValid.Should().BeTrue();
        }

        [Test]
        public void Days_to_claim_cant_be_negative()
        {
            _model.DaysToClaim = -1;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDaysToClaimIsNegative, message);
        }

        [Test]
        public void Zero_length_duration_validation_works()
        {
            _model.DurationType = DurationType.StartDateBased;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDurationIsZeroLength, message);
        }

        [Test]
        public void Duration_is_over_activity_date_range_validation_works()
        {
            _model.DurationType = DurationType.StartDateBased;
            _model.DurationDays = 5;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDurationDaterangeIsIncorrect, message);
        }

        [Test]
        public void Duration_dates_out_of_acivity_date_range_validation_works()
        {
            _model.DurationType = DurationType.Custom;
            _model.DurationStart = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).DateTime;
            _model.DurationEnd = DateTimeOffset.Now.AddDays(2).ToBrandOffset(_brandTimezoneId).DateTime;

            var validationResult = BonusQueries.GetValidationResult(_model);
            var message = validationResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDurationDaterangeIsIncorrect, message);
        }

        [Test]
        public void Can_not_update_non_existing_bonus()
        {
            _model.Id = Guid.NewGuid();

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(ValidatorMessages.BonusDoesNotExist, validationResult.Errors.Single().ErrorMessage);
        }

        [Test]
        public void Can_not_update_not_current_version_bonus()
        {
            var id = Guid.NewGuid();
            BonusRepository.Bonuses.Add(new Core.Data.Bonus { Id = id, Version = 1000 });

            _model.Id = id;

            var validationResult = BonusQueries.GetValidationResult(_model);
            Assert.AreEqual(ValidatorMessages.BonusVersionIsNotCurrent, validationResult.Errors.Single().ErrorMessage);
        }
    }
}