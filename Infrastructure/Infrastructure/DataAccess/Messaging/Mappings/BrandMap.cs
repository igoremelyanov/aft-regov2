using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Core.Messaging.Data.Brand>
    {
        public BrandMap(string schema)
        {
            ToTable("Brands", schema);

            Property(x => x.Name).IsRequired();
            HasMany(x => x.Languages)
                .WithMany()
                .Map(x =>
                {
                    x.MapLeftKey("Id");
                    x.MapRightKey("Code");
                    x.ToTable("xref_BrandLanguages", schema);
                });
        }
    }
}