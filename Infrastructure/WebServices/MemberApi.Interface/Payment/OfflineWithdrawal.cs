using System;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class OfflineWithdrawal
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string AmountFormatted { get; set; }
        public string Created { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string TransactionNumber { get; set; }
    }
}
