using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Commands
{
    public class WithdrawRequestCancel:ICommand
    {
        public string ActorName;
        public Guid WithdrawId;
        public Guid CanceledUserId;
        public string Remarks;
        public WithdrawalStatus Status;
        public DateTimeOffset Canceled;
    }
}
