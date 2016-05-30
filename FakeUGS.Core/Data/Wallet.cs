using System;
using System.Collections.Generic;

namespace FakeUGS.Core.Data
{
    public class Wallet
    {
        public Wallet()
        {
            Id = Guid.NewGuid();
            Transactions = new List<Transaction>();
        }

        public Guid     Id { get; set; }

        public Guid     PlayerId { get; set; }
        public string   CurrencyCode { get; set; }

        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        /// <summary>
        /// The amount available to bet, excluding Bonus Balance
        /// </summary>
        public decimal Balance { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
        
    }


}