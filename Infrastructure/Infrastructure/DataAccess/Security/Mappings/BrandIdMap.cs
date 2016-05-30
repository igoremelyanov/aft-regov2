using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class BrandIdMap : EntityTypeConfiguration<BrandId>
    {
        public BrandIdMap(string schema)
        {
            ToTable("AdminBrands", schema);
            HasKey(b => new { b.AdminId, b.Id });
            
            HasRequired(b => b.Admin)
                .WithMany(u => u.AllowedBrands)
                .WillCascadeOnDelete(true);
        }
    }
}
