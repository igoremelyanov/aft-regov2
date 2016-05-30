using System;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class QualifiedBonus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
		public decimal RequiredAmount { get; set; }
	    public string Percentage { get; set; }
        public decimal Amount { get; set; }
    }
}
