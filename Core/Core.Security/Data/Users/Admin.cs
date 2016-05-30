using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Security.Users;

namespace AFT.RegoV2.Core.Security.Data.Users
{
    public class AdminId
    {
        private readonly Guid _id;

        public AdminId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(AdminId id)
        {
            return id._id;
        }

        public static implicit operator AdminId(Guid id)
        {
            return new AdminId(id);
        }
    }

    public class Admin
    {
        public Admin()
        {
            Licensees = new List<LicenseeId>();
            AllowedBrands = new List<BrandId>();
            Currencies = new List<CurrencyCode>();
            BrandFilterSelections = new List<BrandFilterSelection>();
            LicenseeFilterSelections = new List<LicenseeFilterSelection>();
        }

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Language { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public Role Role { get; set; }

        public ICollection<LicenseeId> Licensees { get; set; }

        public ICollection<BrandId> AllowedBrands { get; set; }

        public ICollection<CurrencyCode> Currencies { get; set; }

        public ICollection<BrandFilterSelection> BrandFilterSelections { get; set; }

        public ICollection<LicenseeFilterSelection> LicenseeFilterSelections { get; set; }
    }
}
