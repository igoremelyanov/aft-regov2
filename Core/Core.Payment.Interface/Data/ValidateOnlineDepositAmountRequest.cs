using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class ValidateOnlineDepositAmountRequest
    {
        public Guid BrandId { get; set; }
        public decimal Amount { get; set; }
	    public Guid PlayerId { get; set; }
    }
}
