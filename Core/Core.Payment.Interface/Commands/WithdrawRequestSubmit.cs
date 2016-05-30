using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Commands
{
    public class WithdrawRequestSubmit : ICommand
    {
        public string ActorName { get; set; }
        public Guid WithdrawId;
        public Guid PlayerId;
        public Guid PlayerBankAccountId;
        public decimal Amount;        
        public Guid LockId;
        public string ReferenceCode;
        public string RequestedBy;
        public DateTimeOffset Requested;
        public string Remarks;
    }
}
