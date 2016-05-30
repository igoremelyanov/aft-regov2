﻿using System;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class WithdrawalWagerChecked : WithdrawalEvent
    {
        public WithdrawalWagerChecked() { }

        public WithdrawalWagerChecked(
            Guid wdId,
            decimal amount,
            DateTimeOffset dtCrated,
            Guid uId,
            Guid wdmadeBy,
            WithdrawalStatus status,
            string remark,
            string transactionNumb,
            string actorname = null) : base(wdId, amount, dtCrated, uId, wdmadeBy, status, remark, transactionNumb, actorname)
        { }
    }
}
