using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class BankAccountEdited : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public Guid BankId { get; set; }
        public BankAccountStatus BankAccountStatus { get; set; }
        public string AccountId { get; set; }
        public string Remarks { get; set; }

        public BankAccountEdited()
        {
        }
    }
}
