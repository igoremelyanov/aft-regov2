using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class GetPlayerPaymentAmountRequest
    {
        public GetPlayerPaymentAmountRequest()
        {
            IsQueryDeposit = true;
            PaymentMethods = new List<string>();
        }
        public bool IsQueryDeposit { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DateApprovedStart { get; set; }
        public DateTime? DateApprovedEnd { get; set; }
        public List<string> PaymentMethods { get; set; }

        public bool IsActive { get; set; }
        public bool IsInactive { get; set; }
        public bool IsTimeOut { get; set; }
        public bool IsSelfExcluded { get; set; }
    }
}
