using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PlayerBankAccountAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
    }
}
