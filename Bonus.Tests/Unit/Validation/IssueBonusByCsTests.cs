using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Validation
{
    internal class IssueByCsTests : UnitTestBase
    {
        [Test]
        public void Bonus_does_not_exist()
        {
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { PlayerId = PlayerId });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Player_does_not_exist()
        {
            var bonus = CreateFirstDepositBonus();
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerDoesNotExist);
        }

        [Test]
        public void Transaction_does_not_exist()
        {
            var bonus = CreateFirstDepositBonus();
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TransactionDoesNotExist);
        }

        [Test]
        public void Deposit_transaction_should_be_passed_with_deposit_bonus()
        {
            MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            transaction.Type = TransactionType.FundIn;
            var bonus = CreateFirstDepositBonus();
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TransactionTypeDoesNotMatchBonusType);
        }

        [Test]
        public void Fundin_transaction_should_be_passed_with_fundin_bonus()
        {
            MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TransactionTypeDoesNotMatchBonusType);
        }

        [Test]
        public void Player_is_not_qualified_for_bonus()
        {
            MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus(isActive: false);
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerIsNotQualifiedForBonus);
        }

        [Test]
        public void Transaction_should_occur_in_bonus_activity_date_range()
        {
            MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus();
            transaction.CreatedOn = bonus.ActiveFrom.AddDays(-1);
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerIsNotQualifiedForBonus);
        }

        [Test]
        public void Player_wallet_should_have_funds_to_lock_for_bonus_with_rollover()
        {
            MakeDeposit(PlayerId, 20);

            PlaceAndLoseBet(10, PlayerId);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 10;
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerHasNoFundsToLockLeft);
        }

        [Test]
        public void Player_wallet_should_have_more_funds_than_wagering_threshold_for_bonus_with_rollover()
        {
            MakeDeposit(PlayerId);

            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus();

            //setting receiving wallet, not the one deposit goes to
            bonus.Template.Info.WalletTemplateId = bonus.Template.Info.Brand.WalletTemplates.Last().Id;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 10;
            bonus.Template.Wagering.Threshold = 60;
            var result = BonusQueries.GetValidationResult(new IssueBonusByCs { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerBalanceIsLessThanWageringThreshold);
        }
    }
}