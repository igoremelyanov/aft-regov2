using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class ValidateOnlineDepositAmount
    {
        public Guid BrandId { get; set; }
        public decimal Amount { get; set; }
    }

    public class ValidateOnlineDepositAmountResponse
    {
        public bool IsValid { get; set; }
    }
}
