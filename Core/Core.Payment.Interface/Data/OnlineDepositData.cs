using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class ApproveOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class VerifyOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class UnverifyOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
        public UnverifyReasons UnverifyReason { get; set; }
    }

    public class RejectOnlineDepositRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
}