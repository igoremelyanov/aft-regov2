using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Bonus.Core.Entities
{
    public class BonusRedemption
    {
        internal readonly Data.BonusRedemption Data;

        internal List<IDomainEvent> Events { get; }

        internal decimal RolloverLeft => Data.Rollover - Data.Contributions.Sum(c => c.Contribution);
        internal Data.Wallet Wallet => Data.Player.Wallets.Single(w => w.Template.Id == Data.Bonus.Template.Info.WalletTemplateId);
        internal bool IsExpired => DateTimeOffset.Now.ToBrandOffset(Data.Player.Brand.TimezoneId) > Data.Bonus.DurationEnd;

        public BonusRedemption(Player player, Bonus bonus, RedemptionParams redemptionParams)
        {
            var bonusReward = bonus.CalculateReward(player, redemptionParams);

            Data = new Data.BonusRedemption
            {
                Amount = bonusReward,
                Player = player.Data,
                Bonus = bonus.Data,
                CreatedOn = SystemTime.Now.ToBrandOffset(bonus.Data.Template.Info.Brand.TimezoneId)
            };
            if (redemptionParams != null)
            {
                Data.Parameters = redemptionParams;
            }

            bonus.Data.Statistic.TotalRedeemedAmount += bonusReward;
            bonus.Data.Statistic.TotalRedemptionCount++;

            Events = new List<IDomainEvent>();
            var redeemedEvent = new BonusRedeemed
            {
                AggregateId = Data.Id,
                BonusId = bonus.Data.Id,
                PlayerId = player.Data.Id,
                BonusName = bonus.Data.Name,
                PlayerName = player.Data.Name,
                Amount = bonusReward,
                CurrencyCode = player.Data.CurrencyCode,
                IssuedByCs = Data.Parameters.IsIssuedByCs
            };

            if (Data.Parameters.IsIssuedByCs == false)
            {
                redeemedEvent.EventCreatedBy = Data.Player.Name;
            }

            Events.Add(redeemedEvent);
        }
        public BonusRedemption(Data.BonusRedemption data)
        {
            Data = data;
            Events = new List<IDomainEvent>();
        }

        public string[] QualifyForActivation()
        {
            return GetQualificationFailures(QualificationPhase.Activation);
        }
        public string[] QualifyForClaiming()
        {
            return GetQualificationFailures(QualificationPhase.Claim);
        }

        private string[] GetQualificationFailures(QualificationPhase phase)
        {
            var bonus = new Bonus(Data.Bonus);
            var player = new Player(Data.Player);

            return bonus.QualifyFor(player, phase, Data.Parameters).ToArray();
        }

        public void MakeClaimable()
        {
            var bonus = new Bonus(Data.Bonus);
            var player = new Player(Data.Player);

            bonus.Data.Statistic.TotalRedeemedAmount -= Data.Amount;
            Data.Amount = bonus.CalculateReward(player, Data.Parameters);
            bonus.Data.Statistic.TotalRedeemedAmount += Data.Amount;

            Data.ActivationState = ActivationStatus.Claimable;
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);

            Events.Add(new RedemptionIsClaimable
            {
                AggregateId = Data.Id
            });
        }
        public Transaction Claim()
        {
            Data.ActivationState = ActivationStatus.Activated;
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);

            var template = Data.Bonus.Template;
            BalanceTarget balanceTarget;
            if (template.Info.IsWithdrawable && template.Wagering.HasWagering == false)
            {
                balanceTarget = BalanceTarget.Main;
            }
            else
            {
                balanceTarget = Data.Bonus.Template.Info.IsWithdrawable == false ? 
                    BalanceTarget.NonTransferableBonus : 
                    BalanceTarget.Bonus;
            }

            var wallet = new Wallet(Wallet);
            var transaction = wallet.IssueBonus(balanceTarget, Data.Amount);

            Events.AddRange(wallet.Events);

            Events.Add(new RedemptionClaimed
            {
                PlayerId = Data.Player.Id,
                AggregateId = Data.Id,
                Amount = Data.Amount,
                CurrencyCode = Data.Player.CurrencyCode
            });

            return transaction;
        }

        public void CompleteRollover()
        {
            Data.RolloverState = RolloverStatus.Completed;
            var trasferedFromBonus = TransferWinningsFromBonusToMain();

            Events.Add(new RedemptionRolloverCompleted
            {
                AggregateId = Data.Id,
                UnlockedAmount = Data.LockedAmount,
                BonusBalanceAdjustment = -trasferedFromBonus,
                MainBalanceAdjustment = trasferedFromBonus,
                CurrencyCode = Data.Player.CurrencyCode
            });
        }

        public void ZeroOutRollover(Transaction transaction)
        {
            Data.Contributions.Add(new RolloverContribution
            {
                Transaction = transaction,
                Contribution = RolloverLeft,
                Type = ContributionType.Threshold
            });
            Data.RolloverState = RolloverStatus.ZeroedOut;
            var trasferedFromBonus = TransferWinningsFromBonusToMain();

            Events.Add(new RedemptionRolloverZeroedOut
            {
                AggregateId = Data.Id,
                UnlockedAmount = Data.LockedAmount,
                BonusBalanceAdjustment = -trasferedFromBonus,
                MainBalanceAdjustment = trasferedFromBonus,
                CurrencyCode = Data.Player.CurrencyCode
            });
        }

        public void Negate(string[] reasons)
        {
            Data.ActivationState = ActivationStatus.Negated;

            RevertRedemptionImplactOnStatistics();
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);

            Events.Add(new RedemptionNegated
            {
                AggregateId = Data.Id,
                Reasons = reasons
            });
        }

        public Transaction Cancel()
        {
            var wallet = new Wallet(Wallet);
            var mainBalance = Wallet.Main;
            var adjustment = new AdjustmentParams(AdjustmentReason.BonusCancelled);

            var transactionsDuringRollover = GetTransactionsDuringRollover();

            var betPlacedDuringRollover = transactionsDuringRollover
                .Where(tr => tr.Type == TransactionType.BetPlaced)
                .ToList();

            var realMoneyContribution = Math.Abs(betPlacedDuringRollover.Sum(tr => tr.MainBalanceAmount));
            var betPlacedTotalDuringRollover = betPlacedDuringRollover.Sum(tr => tr.TotalAmount);

            var betWonTotalDuringRollover = transactionsDuringRollover
                .Where(tr => tr.Type == TransactionType.BetWon)
                .Sum(tr => tr.TotalAmount);

            var netWin = betWonTotalDuringRollover - betPlacedTotalDuringRollover;
            //only net losses should be adjusted
            if (netWin > 0)
            {
                netWin = 0m;
            }
            var mainBalanceAdjustment = realMoneyContribution + netWin;
            if (mainBalanceAdjustment > 0)
            {
                mainBalance += mainBalanceAdjustment;
                adjustment.MainBalanceAdjustment += mainBalanceAdjustment;
            }

            var bonusBalanceAdjustment = betWonTotalDuringRollover + Data.Amount;
            if (bonusBalanceAdjustment > 0)
            {
                var amountFromBonus = Math.Min(wallet.Data.Bonus, bonusBalanceAdjustment);
                adjustment.BonusBalanceAdjustment -= amountFromBonus;

                var amountFromNonWithdrawable = Math.Min(wallet.Data.NonTransferableBonus, bonusBalanceAdjustment - amountFromBonus);
                adjustment.NonTransferableBalanceAdjustment -= amountFromNonWithdrawable;

                var amountFromMain = Math.Min(mainBalance, bonusBalanceAdjustment - amountFromBonus - amountFromNonWithdrawable);
                adjustment.MainBalanceAdjustment -= amountFromMain;
            }

            Data.Contributions.Add(new RolloverContribution
            {
                Contribution = RolloverLeft,
                Type = ContributionType.Cancellation
            });
            Data.ActivationState = ActivationStatus.Canceled;
            Data.RolloverState = RolloverStatus.None;

            RevertRedemptionImplactOnStatistics();
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);

            Events.Add(new RedemptionCanceled
            {
                AggregateId = Data.Id,
                MainBalanceAdjustment = adjustment.MainBalanceAdjustment,
                BonusBalanceAdjustment = adjustment.BonusBalanceAdjustment,
                NonTransferableAdjustment = adjustment.NonTransferableBalanceAdjustment,
                UnlockedAmount = Data.LockedAmount,
                CurrencyCode = Data.Player.CurrencyCode
            });

            var transaction = wallet.AdjustBalances(adjustment);

            Events.AddRange(wallet.Events);
            IssueUnlock();

            return transaction;
        }

        public decimal FulfillRollover(decimal turnoverToDistribute, Transaction transaction)
        {
            var gameId = transaction.GameId.Value;
            var handledAmount = 0m;
            var contributionMultipler = GetGameToWageringContributionMultiplier(gameId);
            if (contributionMultipler > 0m)
            {
                var contributionAmount = turnoverToDistribute * contributionMultipler;
                handledAmount = Math.Min(contributionAmount, RolloverLeft);
                Data.Contributions.Add(new RolloverContribution
                {
                    Transaction = transaction,
                    Contribution = handledAmount,
                    Type = ContributionType.Bet
                });
                Events.Add(new RedemptionRolloverDecreased
                {
                    AggregateId = Data.Id,
                    GameId = gameId,
                    Decreasement = handledAmount,
                    RemainingRollover = RolloverLeft,
                    CurrencyCode = Data.Player.CurrencyCode
                });
            }

            return handledAmount;
        }

        public bool WageringThresholdIsMet(decimal balance)
        {
            return Data.Bonus.Template.Wagering.Threshold >= balance;
        }

        public decimal TransferWinningsFromBonusToMain()
        {
            var amountToTransfer = 0m;
            var transactionsDuringRollover = GetTransactionsDuringRollover();

            if (transactionsDuringRollover.Any())
            {
                var avarageBbBet = transactionsDuringRollover
                    .Where(tr => tr.Type == TransactionType.BetPlaced)
                    .Average(tr => tr.BonusBalanceAmount);

                var netWinFromBb = transactionsDuringRollover.Where(tr => tr.Type == TransactionType.BetWon).Sum(tr => tr.TotalAmount + avarageBbBet);
                var netLossFromBb = transactionsDuringRollover.Where(tr => tr.Type == TransactionType.BetLost).Sum(tr => tr.BonusBalanceAmount);

                var netWin = netWinFromBb + netLossFromBb;
                amountToTransfer = Data.Bonus.Template.Info.IsWithdrawable
                    ? netWin + Data.Amount
                    : netWin;
                if (amountToTransfer > 0)
                {
                    amountToTransfer = Math.Min(amountToTransfer, Wallet.Bonus);
                    if (amountToTransfer > 0)
                    {
                        var wallet = new Wallet(Wallet);
                        wallet.AdjustBalances(new AdjustmentParams(AdjustmentReason.WageringFinished)
                        {
                            MainBalanceAdjustment = amountToTransfer,
                            BonusBalanceAdjustment = -amountToTransfer
                        });
                        Events.AddRange(wallet.Events);
                    }
                }
            }

            return amountToTransfer;
        }

        public void IssueWagering()
        {
            if (Data.Bonus.Template.Wagering.HasWagering == false)
                return;

            CalculateRolloverAmount();
            var computedLocks = ActivateRollover();
            foreach (var lockUnlockParam in computedLocks)
            {
                var walletData = Data.Player.Wallets.Single(w => w.Template.Id == lockUnlockParam.WalletTemplateId);
                var wallet = new Wallet(walletData);
                wallet.Lock(lockUnlockParam.Amount, Data.Id);
            }
        }
        /// <summary>
        /// Calculates and sets the wagering requirement amount that player has to fulfill
        /// according to bonus's terms and conditions
        /// </summary>
        private void CalculateRolloverAmount()
        {
            decimal wageringMethodAmount;
            var wageringMethod = Data.Bonus.Template.Wagering.Method;
            switch (wageringMethod)
            {
                case WageringMethod.Bonus:
                    wageringMethodAmount = Data.Amount;
                    break;
                case WageringMethod.TransferAmount:
                    wageringMethodAmount = Data.Parameters.TransferAmount;
                    break;
                case WageringMethod.BonusAndTransferAmount:
                    wageringMethodAmount = Data.Amount + Data.Parameters.TransferAmount;
                    break;
                default:
                    throw new RegoException($"Wagering method is not valid: {wageringMethod}");
            }

            Data.Rollover = wageringMethodAmount * Data.Bonus.Template.Wagering.Multiplier;
        }

        private List<LockUnlockParams> ActivateRollover()
        {
            Data.RolloverState = RolloverStatus.Active;

            var computedLocks = GetLockUnlockParams();
            Data.LockedAmount = computedLocks.Sum(cl => cl.Amount);

            Events.Add(new RedemptionRolloverIssued
            {
                AggregateId = Data.Id,
                LockedAmount = Data.LockedAmount,
                WageringRequrement = Data.Rollover,
                CurrencyCode = Data.Player.CurrencyCode
            });

            return computedLocks;
        }
        public void IssueUnlock()
        {
            if (Data.LockedAmount <= decimal.Zero) 
                return;

            var computedLocks = GetLockUnlockParams();
            foreach (var lockUnlockParam in computedLocks.GroupBy(l => l.WalletTemplateId))
            {
                var walletData = Data.Player.Wallets.Single(w => w.Template.Id == lockUnlockParam.Key);
                var wallet = new Wallet(walletData);
                wallet.Unlock(Data.Id);
            }
        }

        private List<LockUnlockParams> GetLockUnlockParams()
        {
            var computedLocks = new List<LockUnlockParams>();

            //Data.Parameters.TransferAmount is 0 for bonus types except (First|Reload) deposit and Fund-in
            if (Data.Parameters.TransferAmount > 0 && Data.Parameters.TransferWalletTemplateId.HasValue)
            {
                computedLocks.Add(new LockUnlockParams
                {
                    Amount = Data.Parameters.TransferAmount,
                    WalletTemplateId = Data.Parameters.TransferWalletTemplateId.Value
                });
            }
            //Nothing to lock for IsAfterWager = true bonuses, as no bonus fund were issued at this point
            if (Data.Bonus.Template.Wagering.IsAfterWager == false)
            {
                computedLocks.Add(new LockUnlockParams
                {
                    Amount = Data.Amount,
                    WalletTemplateId = Data.Bonus.Template.Info.WalletTemplateId
                });
            }

            return computedLocks;
        }

        private List<Transaction> GetTransactionsDuringRollover()
        {
            var roundIdsDuringRollover = Data.Contributions
                .Where(tr => tr.Type == ContributionType.Bet)
                .Select(tr => tr.Transaction.RoundId)
                .ToList();

            var transactionsDuringRollover = Wallet
                .Transactions
                .Where(tr => tr.RoundId.HasValue)
                .Where(tr => roundIdsDuringRollover.Contains(tr.RoundId))
                .ToList();

            return transactionsDuringRollover;
        }

        private void RevertRedemptionImplactOnStatistics()
        {
            Data.Bonus.Statistic.TotalRedeemedAmount -= Data.Amount;
            Data.Bonus.Statistic.TotalRedemptionCount--;
        }

        /// <summary>
        /// Returns multiplier using which game contributes to rollover
        /// </summary>
        private decimal GetGameToWageringContributionMultiplier(Guid gameId)
        {
            var contribution = Data.Bonus.Template.Wagering.GameContributions.SingleOrDefault(c => c.GameId == gameId);
            return contribution?.Contribution ?? 1m;
        }
    }
}