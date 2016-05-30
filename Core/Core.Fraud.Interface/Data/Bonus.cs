using System;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class Bonus
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public bool IsActive { get; set; }

        public BonusType BonusType { get; set; }

        public Guid BrandId { get; set; }
    }
}