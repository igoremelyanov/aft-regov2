using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class Wallet: Identity
    {
        public Wallet()
        {
            BonusesRedeemed = new List<BonusRedemption>();
            Transactions = new List<Transaction>();
            Locks = new List<Lock>();
        }

        public virtual WalletTemplate Template { get; set; }

        public virtual List<BonusRedemption> BonusesRedeemed { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
        public virtual List<Lock> Locks { get; set; }

        public virtual Player Player { get; set; }

        public decimal Main { get; set; }

        /// <summary>
        /// Bonus funds from withdawable bonuses and winnings made during wagering requirement fulfillment
        /// </summary>
        public decimal Bonus { get; set; }

        /// <summary>
        /// Bonus funds from not withdawble bonuses. Can not be transferred to Main balance
        /// </summary>
        public decimal NonTransferableBonus { get; set; }

        /// <summary>
        /// The total amount locked by initial bonus conditions. Once wagering requirements are met for a bonus, this lock is removed
        /// </summary>
        public decimal BonusLock { get; set; }
    }

    public enum BalanceTarget
    {
        Main,
        Bonus,
        NonTransferableBonus
    }

    public class AdjustmentParams
    {
        public AdjustmentParams(AdjustmentReason reason)
        {
            Reason = reason;
        }

        public AdjustmentReason Reason { get; set; }
        public decimal MainBalanceAdjustment { get; set; }
        public decimal BonusBalanceAdjustment { get; set; }
        public decimal NonTransferableBalanceAdjustment { get; set; }
        public Guid? RelatedTransactionId { get; set; }
    }

    public enum AdjustmentReason
    {
        BonusCancelled,
        WageringFinished
    }
}