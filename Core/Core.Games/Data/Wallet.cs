using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Data
{
    public class Wallet
    {
        public Wallet()
        {
            Id = Guid.NewGuid();
            Transactions = new List<Transaction>();
            Brand = new Interface.Data.Brand();
        }

        public Guid     Id { get; set; }
        public Guid     PlayerId { get; set; }
        public string   CurrencyCode { get; set; }
        public virtual Interface.Data.Brand Brand { get; set; }
        /// <summary>
        /// The amount available to bet, excluding Bonus Balance
        /// </summary>
        public decimal Balance { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
        
    }
}