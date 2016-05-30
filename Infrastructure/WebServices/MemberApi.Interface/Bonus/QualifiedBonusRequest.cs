namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class QualifiedBonusRequest
    {
	    public QualifiedBonusRequest()
	    {
		    Amount = 0;
	    }

        public decimal? Amount { get; set; }
    }
}
