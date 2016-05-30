using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.MemberApi.Interface.Reports
{
	public class TransactionListResponse
	{
		public IEnumerable<TransactionItemResponse> Items { get; set; }
		public int TotalItemsCount { get; set; }
	}

	public class TransactionItemResponse
	{
		public string Id { get; set; }
		public string GameName { get; set; }
		public decimal Amount { get; set; }
        public string AmountFormatted { get; set; }
		public decimal Bet { get; set; }
        public string BetFormatted { get; set; }
	    public string RoundId { get; set; }
		public DateTimeOffset Date { get; set; }
		public TransactionType TransactionType { get; set; }
	}
}
