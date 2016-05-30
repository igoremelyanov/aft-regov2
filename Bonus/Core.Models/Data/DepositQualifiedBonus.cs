using System;

namespace AFT.RegoV2.Bonus.Core.Models.Data
{
    public class DepositQualifiedBonus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal BonusAmount { get; set; }
        public string Percenage { get; set; }
        public decimal RequiredAmount { get; set; }
    }
}
