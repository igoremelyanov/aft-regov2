using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Brand.Events
{
    public class LanguageUpdated : DomainEventBase
    {
        public LanguageUpdated() { }

        public LanguageUpdated(Culture culture)
        {
            Code = culture.Code;
            Name = culture.Name;
            NativeName = culture.NativeName;
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
    }
}