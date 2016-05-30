using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class MatchingCriteria
    {
        [Key]
        public Guid Key { get; set; }

        public MatchingCriteriaEnum Id { get; set; }

        public string Code { get; set; }
    }
}
