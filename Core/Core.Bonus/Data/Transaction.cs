using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class Transaction : Identity
    {
        public Transaction() { }

        internal Transaction(Wallet wallet)
        {
            MainBalance = wallet.Main;
            BonusBalance = wallet.Bonus;
            NonTransferableBonus = wallet.NonTransferableBonus;
            CreatedOn = DateTimeOffset.Now.ToBrandOffset(wallet.Player.Brand.TimezoneId);
        }

        public TransactionType Type { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal MainBalanceAmount { get; set; }
        public decimal BonusBalanceAmount { get; set; }
        public decimal NonTransferableAmount { get; set; }

        public decimal MainBalance { get; set; }
        public decimal BonusBalance { get; set; }
        public decimal NonTransferableBonus { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public Guid? RoundId { get; set; }
        public Guid? GameId { get; set; }

        /// <summary>
        /// Game Action that triggered the transaction
        /// </summary>
        public Guid? GameActionId { get; set; }

        public Guid? RelatedTransactionId { get; set; }
        public string ReferenceCode { get; set; }
    }
}