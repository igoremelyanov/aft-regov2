using System.Collections.Generic;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class OfflineWithdrawVerificationStatusDTO
    {
        public VerificationDialogHeaderValues VerificationDialogHeaderValues { get; set; }
        public IEnumerable<WithdrawalVerificationLog> ListOfAppliedChecks { get; set; } 
    }
}
