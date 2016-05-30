using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class PlayerLastDepositSummary
    {
        public Guid PlayerId { get; set; }
    }

    public class PlayerLastDepositSummaryResponse
    {
        public string BonusCode { get; set; }
        public decimal Amount { get; set; }
        public decimal? BonusAmount { get; set; }
    }
}
