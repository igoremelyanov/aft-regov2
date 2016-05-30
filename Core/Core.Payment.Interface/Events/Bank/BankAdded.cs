using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class BankAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string BankId { get; set; }
        public Guid BrandId { get; set; }

        public BankAdded() { }
    }
}
