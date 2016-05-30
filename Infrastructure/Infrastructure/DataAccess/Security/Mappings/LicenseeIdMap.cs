using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class LicenseeIdMap : EntityTypeConfiguration<LicenseeId>
    {
        public LicenseeIdMap(string schema)
        {
            ToTable("AdminLicensees", schema);
            HasKey(b => new { b.AdminId, b.Id });

            HasRequired(l => l.Admin)
                .WithMany(u => u.Licensees)
                .WillCascadeOnDelete(true);
        }
    }
}
