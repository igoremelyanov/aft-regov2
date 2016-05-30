using System;
using System.Collections.Generic;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class BonusRedemption: Identity
    {
        public BonusRedemption()
        {
            Contributions = new List<RolloverContribution>();
            Parameters = new RedemptionParams();
        }

        public virtual Player Player { get; set; }
        public virtual Bonus Bonus { get; set; }
        public ActivationStatus ActivationState { get; set; }
        public RolloverStatus RolloverState { get; set; }
        public decimal Amount { get; set; }
        /// <summary>
        /// Total amount locked by rollover
        /// </summary>
        public decimal LockedAmount { get; set; }
        /// <summary>
        /// Is synonym of "Wagering requirement" term
        /// </summary>
        public decimal Rollover { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public RedemptionParams Parameters { get; set; }

        public virtual List<RolloverContribution> Contributions { get; set; }
    }
}