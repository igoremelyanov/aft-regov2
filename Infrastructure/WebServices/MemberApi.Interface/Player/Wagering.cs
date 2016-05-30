using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class WageringRequest
    {
        public Guid? WalletId { get; set; }
    }

    public class WageringResponse
    {
        public decimal Requirement { get; set; }
        public decimal Completed { get; set; }
        public decimal Remaining { get; set; }
    }
}
