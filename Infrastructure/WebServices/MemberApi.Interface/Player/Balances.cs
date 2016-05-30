using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class BalancesRequest 
    {
        public Guid? WalletId { get; set; }
    }

    public class BalancesResponse 
    {
        public decimal Main { get; set; }
        public string MainFormatted { get; set; }
        public decimal Bonus { get; set; }
        public string BonusFormatted { get; set; }
        public decimal Free { get; set; }
        public string FreeFormatted { get; set; }
        public decimal Playable { get; set; }
        public string PlayableFormatted { get; set; }
        public string PlayableFormattedShort { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
    }
}
