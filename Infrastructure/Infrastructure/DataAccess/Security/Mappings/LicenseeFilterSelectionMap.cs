using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class LicenseeFilterSelectionMap : EntityTypeConfiguration<LicenseeFilterSelection>
    {
        public LicenseeFilterSelectionMap(string schema)
        {
            ToTable("LicenseeFilterSelections", schema);
            HasKey(b => new { b.AdminId, b.LicenseeId });
            
            HasRequired(b => b.Admin)
                .WithMany(u => u.LicenseeFilterSelections)
                .WillCascadeOnDelete(true);
        }
    }
}
