using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class RewardTier: Identity
    {
        public RewardTier()
        {
            BonusTiers = new List<TierBase>();
        }

        public string CurrencyCode { get; set; }
        public virtual List<TierBase> BonusTiers { get; set; }
        /// <summary>
        /// Limits aggregated bonus reward per bonus
        /// </summary>
        public decimal RewardAmountLimit { get; set; }

        public List<BonusTier> Tiers => BonusTiers.OfType<BonusTier>().ToList();
        public List<HighDepositTier> HighDepositTiers => BonusTiers.OfType<HighDepositTier>().ToList();
    }
}