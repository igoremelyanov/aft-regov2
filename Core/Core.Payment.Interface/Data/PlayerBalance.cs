using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class PlayerBalance
    {
        public decimal Total;
        public decimal Playable;
        public decimal Main;
        public decimal Bonus;
        public decimal WithdrawalLock;
        public decimal BonusLock;
        public string CurrencyCode;

        /// <summary>
        /// FB = PB - (Greater of BL, BB), 0 if negative
        /// </summary>
        public decimal Free
        {
            get
            {
                var free= Playable - (Math.Max(Bonus,BonusLock));
                if (free < 0)
                    free = 0;
                return free;
            }
        }
    }
}
