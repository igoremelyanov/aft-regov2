using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BrandCurrencyMap : EntityTypeConfiguration<Core.Payment.Data.BrandCurrency>
    {
        public BrandCurrencyMap()
        {
            ToTable("BrandCurrency", Configuration.Schema);
            HasKey(x => new { x.BrandId, x.CurrencyCode });
        }
    }
}
