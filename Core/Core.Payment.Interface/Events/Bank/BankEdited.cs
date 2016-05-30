using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class BankEdited : DomainEventBase
    {
        public Guid Id { get; set; }
        public string BankName { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public Guid BrandId { get; set; }
        public string BankId { get; set; }

        public BankEdited() { }
    }
}
