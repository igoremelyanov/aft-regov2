using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class BankAccountAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string AccountId { get; set; }
        public Guid BankId { get; set; }
        public BankAccountStatus BankAccountStatus { get; set; }
        public string Remarks { get; set; }

        public BankAccountAdded()
        {
        }        
    }
}
