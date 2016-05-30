using AFT.RegoV2.Core.Common.Interfaces;
namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class CurrencyUpdated : DomainEventBase
    {
        public string OldCode { get; set; }
        public string OldName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
    }
}
