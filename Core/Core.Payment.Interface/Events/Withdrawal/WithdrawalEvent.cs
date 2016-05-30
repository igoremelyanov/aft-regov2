using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    /// <summary>
    /// The event is responsible for all the possible events related to a particular withdrawal. E.g. Created, Verified
    /// Rejected, Approved etc.
    /// </summary>
    public class WithdrawalEvent : DomainEventBase
    {
        public WithdrawalEvent() { }

        public WithdrawalEvent(
            Guid wdId,
            decimal amount,
            DateTimeOffset dtCrated,
            Guid uId,
            Guid wdMadeBy,
            WithdrawalStatus status,
            string remark,
            string transactionNumb,
            string username = null)
        {
            this.WithdrawalId = wdId;
            this.Amount = amount;
            this.DateCreated = dtCrated;
            this.UserId = uId;
            this.WithdrawalMadeBy = wdMadeBy;
            this.Status = status;
            this.Remark = remark;
            this.TransactionNumber = transactionNumb;
            this.Username = username ?? EventCreatedBy;
        }

        public Guid WithdrawalId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public Guid UserId { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string Remark { get; set; }
        public string TransactionNumber { get; set; }
        //Kristian: TODO this is temporary. I have to remove it from here after Konstantin is ready with Core.Auth. 
        //Kristian: TODO: There's a task related to that refactoring(https://jira.afusion.com/browse/AFTREGO-3615)
        public string Username { get; set; }

        //Indicates the Id of the player who made the withdrawal request
        public Guid WithdrawalMadeBy { get; set; }
    }
}
