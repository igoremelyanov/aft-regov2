using System;
using System.ComponentModel.DataAnnotations;

using FakeUGS.Core.Extensions;

namespace FakeUGS.Core.Data
{
    public class Transaction
    {
        public Transaction()
        {
            Id = Guid.NewGuid();
        }

        public Transaction(Wallet wallet): this()
        {
            WalletId = wallet.Id;
            MainBalance = wallet.Balance;
            CreatedOn = DateTimeOffset.Now.ToBrandOffset(wallet.Brand.TimezoneId);
        }

        public Guid Id { get; set; }
        public Guid? RoundId { get; set; }
        public Guid? GameId { get; set; }
        public string GameName { get; set; }
        public TransactionType Type { get; set; }
        public decimal MainBalanceAmount { get; set; }

        public decimal MainBalance { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public Guid WalletId { get; set; }
        public Guid? RelatedTransactionId { get; set; }

        [Required]
        public string Description { get; set; }
        public string TransactionNumber { get; set; }

        public Guid? PerformedBy { get; set; }
    }
}