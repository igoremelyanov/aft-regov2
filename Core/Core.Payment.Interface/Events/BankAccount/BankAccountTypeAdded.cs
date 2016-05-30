using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class BankAccountTypeAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
