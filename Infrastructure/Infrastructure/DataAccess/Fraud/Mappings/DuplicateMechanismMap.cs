using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class DuplicateMechanismMap : EntityTypeConfiguration<DuplicateMechanismConfiguration>
    {
        public DuplicateMechanismMap()
        {
            ToTable("DuplicateMechanismConfigurations", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(x => x.Brand)
                .WithMany()
                .HasForeignKey(o => o.BrandId);
        }
    }
}
