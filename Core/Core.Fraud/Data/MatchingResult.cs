using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class MatchingResult
    {
        [Index("PlayerScore_1", Order = 1)]
        [ForeignKey("FirstPlayer")]
        [Key]
        [Column(Order = 1)] 
        public Guid FirstPlayerId { get; set; }
        public Player FirstPlayer { get; set; }

        [Index("PlayerScore_2", Order = 1)]
        [ForeignKey("SecondPlayer")]
        [Key]
        [Column(Order = 2)] 
        public Guid SecondPlayerId { get; set; }
        public Player SecondPlayer { get; set; }

        [Index("PlayerScore_1", Order = 2)]
        [Index("PlayerScore_2", Order = 2)]
        public ICollection<MatchingCriteria> MatchingCriterias { get; set; }
    }
}
