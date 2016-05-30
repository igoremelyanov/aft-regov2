using System;
using System.Collections.Generic;

namespace FakeUGS.Core.Data
{
    public class PlayerBalanceSet
    {
        public List<BalanceSet> WalletsBalanceSet { get; set; }
    }

    public class BalanceSet
    {
        public Guid WalletId { get; set; }
        public string WalletName { get; set; }
        public string WalletCurrency { get; set; }

        public decimal Total { get; set; }

    }
}
