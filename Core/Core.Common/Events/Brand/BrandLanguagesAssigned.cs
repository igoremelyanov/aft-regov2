using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class BrandLanguagesAssigned : DomainEventBase
    {
        public BrandLanguagesAssigned() { } // default constructor is required for publishing event to MQ

        public BrandLanguagesAssigned(Guid id, string name, IEnumerable<Culture> cultures, string defaultCultureCode)
        {
            BrandId = id;
            BrandName = name;
            Languages = cultures.ToList();
            DefaultLanguageCode = defaultCultureCode;
        }

        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public List<Culture> Languages { get; set; }
        public string DefaultLanguageCode { get; set; }
    }
}