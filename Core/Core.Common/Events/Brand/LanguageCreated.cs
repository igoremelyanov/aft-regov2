using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Brand.Events
{
    public class LanguageCreated : DomainEventBase
    {
        public LanguageCreated() { }

        public LanguageCreated(Culture culture)
        {
            Code = culture.Code;
            Name = culture.Name;
            NativeName = culture.NativeName;
            Status = culture.Status;
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        public CultureStatus Status { get; set; }
    }
}