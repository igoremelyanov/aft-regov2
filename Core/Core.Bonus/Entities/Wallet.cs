using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Models.Events.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Bonus.Core.Entities
{
    public class Wallet
    {
        internal IEnumerable<IDomainEvent> Events => _events;

        private readonly List<IDomainEvent> _events;

        internal readonly Data.Wallet Data;
        private bool HasWageringRequirement => Data.BonusesRedeemed.Any(redemption => redemption.RolloverState == RolloverStatus.Active);
        internal decimal TotalBalance => TotalBonus + Data.Main;
        internal decimal TotalBonus => Data.Bonus + Data.NonTransferableBonus;

        public Wallet(Data.Wallet wallet)
        {
            Data = wallet;
            _events = new List<IDomainEvent>();
        }

        public Transaction Deposit(decimal amount, string referenceCode = null)
        {
            ValidateOperationAmount(amount);
            Data.Main += amount;

            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                Type = TransactionType.Deposit,
                MainBalanceAmount = amount,
                ReferenceCode = referenceCode
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }
        public Transaction Withdraw(decimal amount, string referenceCode = null)
        {
            ValidateOperationAmount(amount);
            if (Data.Main < amount)
                throw new RegoException("Insufficient funds");
            Data.Main -= amount;

            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                MainBalanceAmount = -amount,
                Type = TransactionType.Withdraw,
                ReferenceCode = referenceCode
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }

        public Transaction TransferFundCredit(decimal amount)
        {
            ValidateOperationAmount(amount);
            Data.Main += amount;

            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                Type = TransactionType.FundIn,
                MainBalanceAmount = amount
            };

            Data.Transactions.Add(transaction);

            return transaction;
        }
        public Transaction TransferFundDebit(decimal amount)
        {
            ValidateOperationAmount(amount);
            if (Data.Main < amount)
                throw new RegoException("Insufficient funds");
            Data.Main -= amount;

            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                MainBalanceAmount = -amount,
                Type = TransactionType.FundOut
            };

            Data.Transactions.Add(transaction);

            return transaction;
        }

        public Transaction IssueBonus(BalanceTarget target, decimal amount)
        {
            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                Type = TransactionType.Bonus
            };

            if (target == BalanceTarget.Main)
            {
                Data.Main += amount;
                transaction.MainBalanceAmount = amount;
                transaction.MainBalance = Data.Main;
            }
            else
            {
                if (target == BalanceTarget.Bonus)
                {
                    Data.Bonus += amount;
                    transaction.BonusBalanceAmount = amount;
                    transaction.BonusBalance = Data.Bonus;
                }
                else
                {
                    Data.NonTransferableBonus += amount;
                    transaction.NonTransferableAmount = amount;
                    transaction.NonTransferableBonus = Data.NonTransferableBonus;
                }
            }

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }
        public Transaction AdjustBalances(AdjustmentParams adjustment)
        {
            Data.Main += adjustment.MainBalanceAdjustment;
            Data.Bonus += adjustment.BonusBalanceAdjustment;
            Data.NonTransferableBonus += adjustment.NonTransferableBalanceAdjustment;

            if (Data.Bonus < 0 || Data.NonTransferableBonus < 0)
                throw new RegoException("Insufficient funds");

            var transaction = new Transaction(Data)
            {
                TotalAmount = adjustment.MainBalanceAdjustment + adjustment.BonusBalanceAdjustment + adjustment.NonTransferableBalanceAdjustment,
                Type = GetTransactionType(adjustment.Reason),
                MainBalanceAmount = adjustment.MainBalanceAdjustment,
                BonusBalanceAmount = adjustment.BonusBalanceAdjustment,
                NonTransferableAmount = adjustment.NonTransferableBalanceAdjustment,
                RelatedTransactionId = adjustment.RelatedTransactionId
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }

        public Transaction PlaceBet(decimal amount, Guid roundId, Guid gameId, Guid gameActionId)
        {
            ValidateOperationAmount(amount);
            if (TotalBalance < amount)
                throw new RegoException("Insufficient funds");

            var amountToDebit = amount;
            var mainBalanceDebit = Math.Min(Data.Main, amountToDebit);
            amountToDebit -= mainBalanceDebit;
            var bonusBalanceDebit = Math.Min(Data.Bonus, amountToDebit);
            amountToDebit -= bonusBalanceDebit;
            var nonWithdrawableBalanceDebit = Math.Min(Data.NonTransferableBonus, amountToDebit);

            Data.Main -= mainBalanceDebit;
            Data.Bonus -= bonusBalanceDebit;
            Data.NonTransferableBonus -= nonWithdrawableBalanceDebit;

            var transaction = new Transaction(Data)
            {
                Type = TransactionType.BetPlaced,
                TotalAmount = amount,
                MainBalanceAmount = -mainBalanceDebit,
                BonusBalanceAmount = -bonusBalanceDebit,
                NonTransferableAmount = -nonWithdrawableBalanceDebit,
                GameId = gameId,
                GameActionId = gameActionId,
                RoundId = roundId
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }
        public Transaction FreeBet(decimal amount, Guid roundId, Guid gameId, Guid gameActionId)
        {
            ValidateOperationAmount(amount);

            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                Type = TransactionType.BetFree,
                GameId = gameId,
                RoundId = roundId
            };

            if (HasWageringRequirement)
            {
                transaction.BonusBalanceAmount = amount;
                Data.Bonus += amount;
            }
            else
            {
                transaction.MainBalanceAmount = amount;
                Data.Main += amount;
            }

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }
        public Transaction WinBet(Guid roundId, decimal amount, Guid gameActionId)
        {
            ValidateOperationAmount(amount);
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            var mbContributionRounded = 0m;
            decimal bbContributionRounded;
            if (HasWageringRequirement)
            {
                bbContributionRounded = amount;
            }
            else
            {
                var betRiskMb = betPlacedTransactions.Sum(tr => Math.Abs(tr.MainBalanceAmount));
                var betRiskBb = betPlacedTransactions.Sum(tr => Math.Abs(tr.BonusBalanceAmount));
                var betRiskTotal = betRiskMb + betRiskBb;
                mbContributionRounded = Math.Round(amount * (betRiskMb / betRiskTotal), 6);
                bbContributionRounded = Math.Round(amount * (betRiskBb / betRiskTotal), 6);
            }

            Data.Main += mbContributionRounded;
            Data.Bonus += bbContributionRounded;

            var transaction = new Transaction(Data)
            {
                TotalAmount = amount,
                Type = TransactionType.BetWon,
                GameId = betPlacedTransactions.First().GameId,
                GameActionId = gameActionId,
                RoundId = roundId,
                MainBalanceAmount = mbContributionRounded,
                BonusBalanceAmount = bbContributionRounded
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }
        public Transaction LoseBet(Guid roundId, Guid gameActionId)
        {
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            var averageBetAmountFromMb = betPlacedTransactions.Average(tr => tr.MainBalanceAmount);
            var averageBetAmountFromBb = betPlacedTransactions.Average(tr => tr.BonusBalanceAmount);
            var averageBetAmountFromNb = betPlacedTransactions.Average(tr => tr.NonTransferableAmount);

            var transaction = new Transaction(Data)
            {
                TotalAmount = Math.Abs(averageBetAmountFromMb + averageBetAmountFromBb),
                Type = TransactionType.BetLost,
                MainBalanceAmount = averageBetAmountFromMb,
                BonusBalanceAmount = averageBetAmountFromBb,
                NonTransferableAmount = averageBetAmountFromNb,
                GameId = betPlacedTransactions.First().GameId,
                GameActionId = gameActionId,
                RoundId = roundId
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }
        public Transaction CancelBet(Guid transactionId, Guid gameActionId)
        {
            var trxToCancel = Data.Transactions.SingleOrDefault(tr => tr.Id == transactionId);
            if (trxToCancel == null)
            {
                throw new RegoException("Bet with such transactionId does not exist.");
            }
            var trxType = trxToCancel.Type;
            if (trxType != TransactionType.BetPlaced && trxType != TransactionType.BetWon &&
                trxType != TransactionType.BetLost)
            {
                throw new RegoException("Transaction type is not supported.");
            }
            var duplicateCancellation = Data.Transactions.Any(tr =>
                    tr.Type == TransactionType.BetCancelled &&
                    tr.RelatedTransactionId == trxToCancel.Id);
            if (duplicateCancellation)
            {
                throw new RegoException("Bet was already canceled.");
            }

            Data.Main -= trxToCancel.MainBalanceAmount;
            Data.Bonus -= trxToCancel.BonusBalanceAmount;
            Data.NonTransferableBonus -= trxToCancel.NonTransferableAmount;

            var transaction = new Transaction(Data)
            {
                TotalAmount = -(trxToCancel.MainBalanceAmount + trxToCancel.BonusBalanceAmount + trxToCancel.NonTransferableAmount),
                Type = TransactionType.BetCancelled,
                GameId = trxToCancel.GameId,
                GameActionId = gameActionId,
                RoundId = trxToCancel.RoundId,
                RelatedTransactionId = trxToCancel.Id,
                MainBalanceAmount = -trxToCancel.MainBalanceAmount,
                BonusBalanceAmount = -trxToCancel.BonusBalanceAmount,
                NonTransferableAmount = -trxToCancel.NonTransferableAmount
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }

        public Transaction AdjustTransaction(Guid transactionId, decimal newAmount, Guid gameActionId)
        {
            var trxToAdjust = Data.Transactions.SingleOrDefault(tr => tr.Id == transactionId);
            if (trxToAdjust == null)
            {
                throw new RegoException("Bet with such transactionId does not exist.");
            }
            var trxType = trxToAdjust.Type;
            if (trxType != TransactionType.BetWon)
            {
                throw new InvalidOperationException("Transaction type is not supported.");
            }
            var duplicateAdjustment = Data.Transactions.Any(tr =>
                    tr.Type == TransactionType.BetWonAdjustment &&
                    tr.RelatedTransactionId == trxToAdjust.Id);
            if (duplicateAdjustment)
            {
                throw new InvalidOperationException("Transaction was already adjusted.");
            }

            Func<decimal, decimal> percentOf = x => Math.Round((x / trxToAdjust.TotalAmount) * newAmount, 6);

            var adjustmentToMain = percentOf(trxToAdjust.MainBalanceAmount);
            var adjustmentToBonus = percentOf(trxToAdjust.BonusBalanceAmount);

            Data.Main += adjustmentToMain;
            Data.Bonus += adjustmentToBonus;

            var transaction = new Transaction(Data)
            {
                TotalAmount = newAmount,
                Type = TransactionType.BetWonAdjustment,
                GameId = trxToAdjust.GameId,
                RoundId = trxToAdjust.RoundId,
                RelatedTransactionId = trxToAdjust.Id,
                MainBalanceAmount = adjustmentToMain,
                BonusBalanceAmount = adjustmentToBonus
            };

            Data.Transactions.Add(transaction);
            AddBonusWalletBalanceChangedEvent(transaction);

            return transaction;
        }

        public Lock Lock(decimal amount, Guid redemptionId)
        {
            ValidateOperationAmount(amount);

            Data.BonusLock += amount;
            var lockData = new Lock
            {
                Amount = amount,
                RedemptionId = redemptionId,
                LockedOn = DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId)
            };
            Data.Locks.Add(lockData);

            return lockData;
        }
        public List<Lock> Unlock(Guid redemptionId)
        {
            var locks = Data.Locks.Where(l => l.RedemptionId == redemptionId).ToList();
            if (locks.Any() == false)
                throw new RegoException("Locks not found");

            foreach (var lockData in locks)
            {
                if (lockData.UnlockedOn.HasValue)
                    throw new RegoException("Lock was released");

                Data.BonusLock -= lockData.Amount;
                lockData.UnlockedOn = DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId);
            }

            return locks;
        }

        private void ValidateOperationAmount(decimal amount)
        {
            if (amount <= 0)
                throw new RegoException("Invalid amount");
        }
        private TransactionType GetTransactionType(AdjustmentReason adjustmentReason)
        {
            switch (adjustmentReason)
            {
                case AdjustmentReason.BonusCancelled:
                    return TransactionType.BonusCancelled;
                case AdjustmentReason.WageringFinished:
                    return TransactionType.WageringFinished;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private List<Transaction> GetBetPlacedTransactions(Guid roundId)
        {
            var betPlacedTransactions =
                Data.Transactions.Where(tr => tr.RoundId == roundId && tr.Type == TransactionType.BetPlaced).ToList();
            if (betPlacedTransactions.Any() == false)
            {
                throw new RegoException("No bets were placed with this roundId.");
            }

            return betPlacedTransactions;
        }

        private void AddBonusWalletBalanceChangedEvent(Transaction transaction)
        {
            _events.Add(new BonusWalletBalanceChanged
            {
                Wallet = new WalletData
                {
                    Id = Data.Id,
                    PlayerId = Data.Player.Id,
                    Balance = TotalBalance,
                    CurrencyCode = Data.Player.CurrencyCode
                },
                TransactionId = transaction.Id,
                RelatedTransactionId = transaction.RelatedTransactionId,
                Type = transaction.Type,
                MainBalanceAmount = transaction.MainBalanceAmount,
                BonusBalanceAmount = transaction.BonusBalanceAmount + transaction.NonTransferableAmount,
                RoundId = transaction.RoundId,
                GameId = transaction.GameId,
                CreatedOn = transaction.CreatedOn,
                TransactionNumber = transaction.ReferenceCode
            });
        }
    }
}