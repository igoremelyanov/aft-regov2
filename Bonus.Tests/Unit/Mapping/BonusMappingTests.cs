using System;
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
    internal class MappingTests : UnitTestBase
    {
        private CreateUpdateBonus _model;
        private BonusMapper _bonusMapper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var template = CreateFirstDepositTemplate();
            _model = new CreateUpdateBonus
            {
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                TemplateId = template.Id,
                ActiveFrom = DateTimeOffset.Now.Date,
                ActiveTo = DateTimeOffset.Now.AddDays(1).Date,
                DurationType = DurationType.None
            };

            _bonusMapper = Container.Resolve<BonusMapper>();
        }

        [Test]
        public void Mapper_assigns_bonus_template()
        {
            var bonus = _bonusMapper.MapModelToBonus(_model);

            bonus.Template.Should().NotBeNull();
        }

        [Test]
        public void None_duration_uses_activity_range_dates()
        {
            var bonus = _bonusMapper.MapModelToBonus(_model);

            bonus.ActiveFrom.Should().Be(bonus.DurationStart);
            bonus.ActiveTo.Should().Be(bonus.DurationEnd);
        }

        [Test]
        public void Custom_duration_is_mapped_using_provided_values()
        {
            _model.DurationStart = _model.ActiveFrom.AddHours(1);
            _model.DurationEnd = _model.ActiveTo.AddHours(-1);
            _model.DurationType = DurationType.Custom;
            var bonus = _bonusMapper.MapModelToBonus(_model);

            bonus.DurationStart.DateTime.Should().Be(_model.DurationStart);
            bonus.DurationEnd.DateTime.Should().Be(_model.DurationEnd);
        }

        [Test]
        public void Duration_based_on_active_from_date_correctly_translates_to_duration_end_date()
        {
            _model.ActiveTo = DateTimeOffset.Now.AddDays(2).Date;
            _model.DurationType = DurationType.StartDateBased;
            _model.DurationDays = 1;
            _model.DurationHours = 2;
            _model.DurationMinutes = 3;

            var bonus = _bonusMapper.MapModelToBonus(_model);

            var expectedDurationEnd = _model.ActiveFrom
                .AddDays(_model.DurationDays)
                .AddHours(_model.DurationHours)
                .AddMinutes(_model.DurationMinutes);

            bonus.DurationEnd.DateTime.Should().Be(expectedDurationEnd);
        }
    }
}
