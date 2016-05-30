using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Commands
{
    public class WithdrawRequestApprove: ICommand
    {
        public string ActorName { get; set; }
        public Guid WithdrawId;        
        public string Remarks;
        public Guid ApprovedUerId;
        public DateTimeOffset Approved;
    }
}
