using System.Collections.Generic;
using System;
namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class FirstDepositApplicationRequest
    {
        public decimal DepositAmount { get; set; }
        public string BonusCode { get; set; }
        public Guid PlayerId { get; set; }
    }

    public class FirstDepositApplicationResponse
    {
	    public FirstDepositApplicationResponse()
	    {
		    Errors = new List<string>();
	    }

        public bool IsValid { get; set; }
        public QualifiedBonus Bonus { get; set; }
	    public IEnumerable<string> Errors { get; set; } 
    }
}
