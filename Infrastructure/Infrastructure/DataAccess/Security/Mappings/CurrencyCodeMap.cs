using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class CurrencyCodeMap : EntityTypeConfiguration<CurrencyCode>
    {
        public CurrencyCodeMap(string schema)
        {
            ToTable("AdminCurrencies", schema);
            HasKey(b => new { b.AdminId, b.Currency });

            HasRequired(b => b.Admin)
                .WithMany(u => u.Currencies)
                .WillCascadeOnDelete(true);
        }
    }
}
