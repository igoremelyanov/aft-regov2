using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class CurrencyCreated : DomainEventBase
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public CurrencyStatus Status { get; set; }
    }
}
