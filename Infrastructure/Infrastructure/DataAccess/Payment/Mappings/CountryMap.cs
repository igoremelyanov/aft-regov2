using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class CountryMap : EntityTypeConfiguration<Country>
    {
        public CountryMap()
        {
            ToTable("Countries", Configuration.Schema);
            HasKey(c => c.Code);
        }
    }
}