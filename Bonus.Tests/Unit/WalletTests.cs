using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit
{
    [Category("Unit")]
    internal class WalletTests : TestBase
    {
        private Wallet _walletData;
        private Core.Entities.Wallet _wallet;

        public override void BeforeEach()
        {
            _walletData = new Wallet { Player = new Player { Brand = new Brand { TimezoneId = "Pacific Standard Time" } } };
            _wallet = new Core.Entities.Wallet(_walletData);
        }

        [Test]
        public void Deposit_increases_main_balance()
        {
            var transaction = _wallet.Deposit(200);

            _walletData.Main.Should().Be(200);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.Deposit);
            transaction.TotalAmount.Should().Be(200);
            transaction.MainBalanceAmount.Should().Be(200);
            transaction.BonusBalanceAmount.Should().Be(0);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Invalid_amount_deposit_throws(decimal amount)
        {
            Action action = () => { _wallet.Deposit(amount); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Withdraw_decreases_main_balance()
        {
            _wallet.Deposit(200);
            var transaction = _wallet.Withdraw(100);

            _walletData.Main.Should().Be(100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.Withdraw);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(-100);
            transaction.BonusBalanceAmount.Should().Be(0);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Invalid_amount_withdraw_throws(decimal amount)
        {
            Action action = () => { _wallet.Withdraw(amount); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Can_not_withdraw_if_there_are_no_funds_in_main()
        {
            Action action = () => { _wallet.Withdraw(50); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Fund_transfer_debit_decreases_main()
        {
            _wallet.Deposit(200);
            var transaction = _wallet.TransferFundDebit(100);

            _walletData.Main.Should().Be(100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.FundOut);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(-100);
            transaction.BonusBalanceAmount.Should().Be(0);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Invalid_amount_fund_transfer_debit_throws(decimal amount)
        {
            Action action = () => { _wallet.TransferFundDebit(amount); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Can_not_fund_transfer_debit_if_there_are_no_funds_in_main()
        {
            Action action = () => { _wallet.TransferFundDebit(50); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Fund_transfer_credit_increases_main()
        {
            var transaction = _wallet.TransferFundCredit(100);

            _walletData.Main.Should().Be(100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.FundIn);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(100);
            transaction.BonusBalanceAmount.Should().Be(0);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Invalid_amount_fund_transfer_credit_throws(decimal amount)
        {
            Action action = () => { _wallet.TransferFundCredit(amount); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Can_issue_bonus_on_main_balance()
        {
            var transaction = _wallet.IssueBonus(BalanceTarget.Main, 100);

            _walletData.Main.Should().Be(100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.Bonus);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(100);
            transaction.BonusBalanceAmount.Should().Be(0);
        }

        [Test]
        public void Can_issue_bonus_on_bonus_balance()
        {
            var transaction = _wallet.IssueBonus(BalanceTarget.Bonus, 100);

            _walletData.Main.Should().Be(0);
            _walletData.Bonus.Should().Be(100);

            transaction.Type.Should().Be(TransactionType.Bonus);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(0);
            transaction.BonusBalanceAmount.Should().Be(100);
        }

        [Test]
        public void Can_issue_bonus_on_nonTransferable_balance()
        {
            var transaction = _wallet.IssueBonus(BalanceTarget.NonTransferableBonus, 100);

            _walletData.Main.Should().Be(0);
            _walletData.Bonus.Should().Be(0);
            _walletData.NonTransferableBonus.Should().Be(100);

            transaction.Type.Should().Be(TransactionType.Bonus);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(0);
            transaction.BonusBalanceAmount.Should().Be(0);
            transaction.NonTransferableAmount.Should().Be(100);
        }

        [Test]
        public void Can_adjust_funds_between_balances_when_wagering_is_finished()
        {
            _wallet.IssueBonus(BalanceTarget.Bonus, 100);
            var transaction = _wallet.AdjustBalances(new AdjustmentParams(AdjustmentReason.WageringFinished)
            {
                BonusBalanceAdjustment = -100,
                MainBalanceAdjustment = 100
            });

            _walletData.Main.Should().Be(100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.WageringFinished);
            transaction.TotalAmount.Should().Be(0);
            transaction.MainBalanceAmount.Should().Be(100);
            transaction.BonusBalanceAmount.Should().Be(-100);
        }

        [Test]
        public void Can_adjust_funds_between_balances_when_bonus_is_cancelled()
        {
            _wallet.IssueBonus(BalanceTarget.Bonus, 100);
            var transaction = _wallet.AdjustBalances(new AdjustmentParams(AdjustmentReason.BonusCancelled)
            {
                BonusBalanceAdjustment = -100,
                MainBalanceAdjustment = 100
            });

            _walletData.Main.Should().Be(100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BonusCancelled);
            transaction.TotalAmount.Should().Be(0);
            transaction.MainBalanceAmount.Should().Be(100);
            transaction.BonusBalanceAmount.Should().Be(-100);
        }

        [Test]
        public void Can_adjust_main_balance_to_be_negative()
        {
            _wallet.IssueBonus(BalanceTarget.Bonus, 100);
            var transaction = _wallet.AdjustBalances(new AdjustmentParams(AdjustmentReason.BonusCancelled)
            {
                BonusBalanceAdjustment = -100,
                MainBalanceAdjustment = -100
            });

            _walletData.Main.Should().Be(-100);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BonusCancelled);
            transaction.TotalAmount.Should().Be(-200);
            transaction.MainBalanceAmount.Should().Be(-100);
            transaction.BonusBalanceAmount.Should().Be(-100);
        }

        [Test]
        public void Can_not_adjust_bonus_balance_to_be_negative()
        {
            Action action = () => _wallet.AdjustBalances(new AdjustmentParams(AdjustmentReason.BonusCancelled)
            {
                BonusBalanceAdjustment = -100,
                MainBalanceAdjustment = 100
            });
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Place_bet_takes_funds_from_both_balances()
        {
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);
            var transaction = PlaceBet(227);

            _walletData.Main.Should().Be(0);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BetPlaced);
            transaction.TotalAmount.Should().Be(227);
            transaction.MainBalanceAmount.Should().Be(-200);
            transaction.BonusBalanceAmount.Should().Be(-27);
        }

        [Test]
        public void Lose_bet_creates_transaction()
        {
            var roundId = Guid.NewGuid();
            _wallet.Deposit(27);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);
            PlaceBet(54, roundId);
            var transaction = _wallet.LoseBet(roundId, Guid.NewGuid());

            _walletData.Main.Should().Be(0);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BetLost);
            transaction.TotalAmount.Should().Be(54);
            transaction.MainBalanceAmount.Should().Be(-27);
            transaction.BonusBalanceAmount.Should().Be(-27);
        }

        [Test]
        public void Losing_sub_bets_creates_transaction_with_proportional_balance_loss()
        {
            var roundId = Guid.NewGuid();
            _wallet.Deposit(150);
            _wallet.IssueBonus(BalanceTarget.Bonus, 150);
            PlaceBet(100, roundId);
            PlaceBet(100, roundId);
            PlaceBet(100, roundId);
            var transaction = _wallet.LoseBet(roundId, Guid.NewGuid());

            _walletData.Main.Should().Be(0);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BetLost);
            transaction.TotalAmount.Should().Be(100);
            transaction.MainBalanceAmount.Should().Be(-50);
            transaction.BonusBalanceAmount.Should().Be(-50);
        }

        [Test]
        public void Can_cancel_bet()
        {
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);
            _wallet.IssueBonus(BalanceTarget.NonTransferableBonus, 27);
            var betTransaction = PlaceBet(254);
            var transaction = _wallet.CancelBet(betTransaction.Id, Guid.NewGuid());

            _walletData.Main.Should().Be(200);
            _walletData.Bonus.Should().Be(27);
            _walletData.NonTransferableBonus.Should().Be(27);

            transaction.Type.Should().Be(TransactionType.BetCancelled);
            transaction.TotalAmount.Should().Be(254);
            transaction.MainBalanceAmount.Should().Be(200);
            transaction.BonusBalanceAmount.Should().Be(27);
            transaction.NonTransferableAmount.Should().Be(27);
        }

        [Test]
        public void Cancel_bet_can_lead_to_negative_balance()
        {
            var roundId = Guid.NewGuid();
            _wallet.Deposit(200);
            PlaceBet(200, roundId);
            var betTransaction = _wallet.WinBet(roundId, 400, Guid.NewGuid());
            _wallet.Withdraw(400);
            var transaction = _wallet.CancelBet(betTransaction.Id, Guid.NewGuid());

            _walletData.Main.Should().Be(-400);
            _walletData.Bonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BetCancelled);
            transaction.TotalAmount.Should().Be(-400);
            transaction.MainBalanceAmount.Should().Be(-400);
            transaction.BonusBalanceAmount.Should().Be(0);
        }

        [Test]
        public void Can_not_cancel_bet_twice()
        {
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);
            var betTransaction = PlaceBet(227);
            _wallet.CancelBet(betTransaction.Id, Guid.NewGuid());
            Action action = () => _wallet.CancelBet(betTransaction.Id, Guid.NewGuid());
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Can_not_cancel_non_bet_transaction()
        {
            var depositTransaction = _wallet.Deposit(200);
            Action action = () => _wallet.CancelBet(depositTransaction.Id, Guid.NewGuid());
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Cancel_non_existent_bet_throws()
        {
            Action action = () => _wallet.CancelBet(Guid.NewGuid(), Guid.NewGuid());
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Adjust_a_win()
        {
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);

            var roundId = Guid.NewGuid();
            PlaceBet(227, roundId);

            var winTransaction = _wallet.WinBet(roundId, 227, Guid.NewGuid());
            var transaction = _wallet.AdjustTransaction(winTransaction.Id, 454, Guid.NewGuid());

            _walletData.Main.Should().Be(600);
            _walletData.Bonus.Should().Be(81);
            _walletData.NonTransferableBonus.Should().Be(0);

            transaction.Type.Should().Be(TransactionType.BetWonAdjustment);
            transaction.TotalAmount.Should().Be(454);
            transaction.MainBalanceAmount.Should().Be(400);
            transaction.BonusBalanceAmount.Should().Be(54);
            transaction.NonTransferableAmount.Should().Be(0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Can_not_adjust_a_win_twice()
        {
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);

            var roundId = Guid.NewGuid();
            PlaceBet(227, roundId);

            var winTransaction = _wallet.WinBet(roundId, 227, Guid.NewGuid());
            _wallet.AdjustTransaction(winTransaction.Id, 454, Guid.NewGuid());
            _wallet.AdjustTransaction(winTransaction.Id, 454, Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Can_not_adjust_not_win_transaction()
        {
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 27);

            var roundId = Guid.NewGuid();
            var betTransaction = PlaceBet(227, roundId);

            _wallet.AdjustTransaction(betTransaction.Id, 300, Guid.NewGuid());
        }

        [Test]
        public void Winning_from_bet_is_credited_proportionally()
        {
            var roundId = Guid.NewGuid();
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 200);
            PlaceBet(400, roundId);
            var transaction = _wallet.WinBet(roundId, 1000, Guid.NewGuid());

            _walletData.Main.Should().Be(500);
            _walletData.Bonus.Should().Be(500);

            transaction.Type.Should().Be(TransactionType.BetWon);
            transaction.TotalAmount.Should().Be(1000);
            transaction.MainBalanceAmount.Should().Be(500);
            transaction.BonusBalanceAmount.Should().Be(500);
        }

        [Test]
        public void Winning_from_sub_bet_is_credited_proportionally()
        {
            var roundId = Guid.NewGuid();
            _wallet.Deposit(150);
            _wallet.IssueBonus(BalanceTarget.Bonus, 150);
            PlaceBet(100, roundId);
            PlaceBet(100, roundId);
            PlaceBet(100, roundId);
            var transaction = _wallet.WinBet(roundId, 1000, Guid.NewGuid());

            _walletData.Main.Should().Be(500);
            _walletData.Bonus.Should().Be(500);

            transaction.Type.Should().Be(TransactionType.BetWon);
            transaction.TotalAmount.Should().Be(1000);
            transaction.MainBalanceAmount.Should().Be(500);
            transaction.BonusBalanceAmount.Should().Be(500);
        }

        [Test]
        public void Winning_from_bet_is_credited_to_bonus_during_rollover()
        {
            _walletData.BonusesRedeemed.Add(new BonusRedemption { RolloverState = RolloverStatus.Active });

            var roundId = Guid.NewGuid();
            _wallet.Deposit(200);
            _wallet.IssueBonus(BalanceTarget.Bonus, 200);
            PlaceBet(400, roundId);
            var transaction = _wallet.WinBet(roundId, 1000, Guid.NewGuid());

            _walletData.Main.Should().Be(0);
            _walletData.Bonus.Should().Be(1000);

            transaction.Type.Should().Be(TransactionType.BetWon);
            transaction.TotalAmount.Should().Be(1000);
            transaction.MainBalanceAmount.Should().Be(0);
            transaction.BonusBalanceAmount.Should().Be(1000);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Invalid_amount_win_bet_throws(decimal amount)
        {
            Action action = () => { _wallet.WinBet(Guid.Empty, amount, Guid.NewGuid()); };
            action.ShouldThrow<Exception>();
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Invalid_amount_lock_throws(decimal amount)
        {
            Action action = () => { _wallet.Lock(amount, Guid.Empty); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Lock_increases_BonusLock()
        {
            var lockData = _wallet.Lock(500, Guid.NewGuid());

            _walletData.BonusLock.Should().Be(500);

            lockData.Amount.Should().Be(500);
            lockData.RedemptionId.Should().NotBe(Guid.Empty);
            lockData.UnlockedOn.Should().NotHaveValue();
        }

        [Test]
        public void Unlock_for_invalid_redemptionId_throws()
        {
            Action action = () => { _wallet.Unlock(Guid.Empty); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Duplicate_unlock_throws()
        {
            _wallet.Lock(500, Guid.Empty);
            _wallet.Unlock(Guid.Empty);
            Action action = () => { _wallet.Unlock(Guid.Empty); };
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void Unlock_decreases_BonusLock()
        {
            _wallet.Lock(500, Guid.Empty);
            _wallet.Lock(1000, Guid.Empty);
            var locks = _wallet.Unlock(Guid.Empty);

            _walletData.BonusLock.Should().Be(0);

            var lock1 = locks.Single(l => l.Amount == 500);
            lock1.RedemptionId.Should().Be(Guid.Empty);
            lock1.UnlockedOn.Should().HaveValue();

            var lock2 = locks.Single(l => l.Amount == 1000);
            lock2.RedemptionId.Should().Be(Guid.Empty);
            lock2.UnlockedOn.Should().HaveValue();
        }

        private Transaction PlaceBet(decimal amount, Guid? roundId = null, Guid? gameId = null, Guid? gameActionId = null)
        {
            if (!roundId.HasValue) roundId = Guid.NewGuid();
            if (!gameId.HasValue) gameId = Guid.NewGuid();
            if (!gameActionId.HasValue) gameActionId = Guid.NewGuid();

            return _wallet.PlaceBet(amount, roundId.Value, gameId.Value, gameActionId.Value);
        }
    }
}
