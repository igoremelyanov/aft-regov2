using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class OfflineDepositRequest
    {
        public Guid PlayerId { get; set; }
        public Guid BankAccountId { get; set; }
        public decimal Amount { get; set; }
        public Guid? BonusId { get; set; }
        public string BonusCode { get; set; }
        public string PlayerRemark { get; set; }
        public NotificationMethod NotificationMethod { get; set; }
    }
}