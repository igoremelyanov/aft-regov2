using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Wallet
{
    public class BonusWalletBalanceChanged : DomainEventBase
    {
        public WalletData Wallet { get; set; }

        public Guid TransactionId { get; set; }
        public Guid? RelatedTransactionId { get; set; }
        public decimal MainBalanceAmount { get; set; }
        public decimal BonusBalanceAmount { get; set; }

        public TransactionType Type { get; set; }

        public Guid? RoundId { get; set; }
        public Guid? GameId { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public Guid? PerformedBy { get; set; }
        public string TransactionNumber { get; set; }

    }

    public class WalletData
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Balance { get; set; }
    }
}
