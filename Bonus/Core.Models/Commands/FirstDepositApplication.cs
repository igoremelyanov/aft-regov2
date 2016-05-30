using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class FirstDepositApplication 
    {
        public Guid PlayerId { get; set; }
        public decimal DepositAmount { get; set; }
        public string BonusCode { get; set; }
    }
}
