using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class BrandFilterSelectionMap : EntityTypeConfiguration<BrandFilterSelection>
    {
        public BrandFilterSelectionMap(string schema)
        {
            ToTable("BrandFilterSelections", schema);
            HasKey(b => new { b.AdminId, b.BrandId });
            
            HasRequired(b => b.Admin)
                .WithMany(u => u.BrandFilterSelections)
                .WillCascadeOnDelete(true);
        }
    }
}
