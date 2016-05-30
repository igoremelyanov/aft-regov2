using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LanguageStatusChanged : DomainEventBase
    {
        public string Code { get; set; }
        public CultureStatus Status { get; set; }
        public string Remarks { get; set; }
    }
}