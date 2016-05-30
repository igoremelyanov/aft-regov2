using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Qualification
{
    internal class IssanceByCsQualificationTests : UnitTestBase
    {
        [Test]
        public void ManualByCs_bonus_is_qualified_during_manual_issuance()
        {
            MakeDeposit(PlayerId);
            CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void Expired_bonus_is_qualifed_when_issued_by_Cs()
        {
            MakeDeposit(PlayerId);
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.ActiveFrom = bonus.ActiveFrom.AddDays(-1);
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-1);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void Bonus_that_become_active_in_future_is_not_qualified_when_issued_by_Cs()
        {
            MakeDeposit(PlayerId);
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.ActiveFrom = bonus.ActiveFrom.AddDays(2);
            bonus.ActiveTo = bonus.ActiveTo.AddDays(2);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Bonus_issued_by_Cs_ignores_bonus_application_duration_qualification()
        {
            MakeDeposit(PlayerId);
            var bonus = CreateFirstDepositBonus();
            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);

            SystemTime.Factory = () => DateTimeOffset.Now.AddMinutes(11);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotBeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now;
        }
    }
}