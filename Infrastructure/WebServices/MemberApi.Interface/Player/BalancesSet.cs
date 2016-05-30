using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class BalanceSetRequest
    {
        public Guid PlayerId { get; set; }
    }

    public class BalanceSetResponse
    {
        //public Dictionary<Guid, BalanceSet> WalletsBalanceSet { get; set; }
        public List<BalanceSet> WalletsBalanceSet { get; set; }
    }

    public class BalanceSet
    {
        public Guid WalletId { get; set; }
        public string WalletName { get; set; }
        public string WalletCurrency { get; set; }

        public decimal Main { get; set; }
        public decimal Bonus { get; set; }
        public decimal Free { get; set; }
        public decimal Playable { get; set; }

        public decimal WageringRequirement { get; set; }
        public decimal WageringCompleted { get; set; }
        public decimal WageringRemaining { get; set; }

    }
}
