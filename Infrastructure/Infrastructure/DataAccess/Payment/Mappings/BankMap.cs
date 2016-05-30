using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BankMap : EntityTypeConfiguration<Bank>
    {
        public BankMap()
        {
            ToTable("Banks", Configuration.Schema);

            HasKey(p => p.Id);

            HasRequired(p => p.Brand)
                .WithMany()
                .HasForeignKey(x => x.BrandId);

            HasRequired(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryCode);

            HasRequired(p => p.Brand)
                .WithMany()
                .WillCascadeOnDelete(false);
            
            HasMany(p => p.Accounts)
                .WithRequired(x => x.Bank);
        }
    }
}