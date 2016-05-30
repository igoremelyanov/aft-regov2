using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LicenseeUpdated : DomainEventBase
    {
        public LicenseeUpdated() { } // default constructor is required for publishing event to MQ

        public LicenseeUpdated(Licensee licensee)
        {
            Id = licensee.Id;
            Name = licensee.Name;
            CompanyName = licensee.CompanyName;
            Email = licensee.Email;
            AffiliateSystem = licensee.AffiliateSystem;
            ContractStart = licensee.ContractStart;
            ContractEnd = licensee.ContractEnd;
            Languages = licensee.Cultures.Select(x => x.Code);
            Countries = licensee.Countries.Select(x => x.Code);
            Currencies = licensee.Currencies.Select(x => x.Code);
            Products = licensee.Products.Select(x => x.ProductId);
            Remarks = licensee.Remarks;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public bool AffiliateSystem { get; set; }
        public DateTimeOffset ContractStart { get; set; }
        public DateTimeOffset? ContractEnd { get; set; }
        public IEnumerable<string> Languages { get; set; }
        public IEnumerable<string> Countries { get; set; }
        public IEnumerable<string> Currencies { get; set; }
        public IEnumerable<Guid> Products { get; set; }
        public string Remarks { get; set; }
    }
}