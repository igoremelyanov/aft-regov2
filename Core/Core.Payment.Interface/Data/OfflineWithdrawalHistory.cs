using System;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class OfflineWithdrawalHistory
    {
        public Guid Id { get; set; }

        public Guid OfflineWithdrawalId { get; set; }

        public WithdrawalStatus Action { get; set; }

        public Guid UserId { get; set; }

        //Kristian: TODO this is temporary. I have to remove it from here after Konstantin is ready with Core.Auth. 
        //Kristian: TODO: There's a task related to that refactoring(https://jira.afusion.com/browse/AFTREGO-3615)
        public string Username { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public string Remark { get; set; }

        public string TransactionNumber { get; set; }

        public virtual OfflineWithdraw OfflineWithdraw { get; set; }
    }
}