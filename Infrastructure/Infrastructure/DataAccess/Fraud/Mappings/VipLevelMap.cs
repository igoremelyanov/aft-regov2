using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class VipLevelMap : EntityTypeConfiguration<Core.Fraud.Interface.Data.VipLevel>
    {
        public VipLevelMap()
        {
            ToTable("VipLevels", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(o => o.Brand)
                .WithMany()
                .HasForeignKey(o => o.BrandId);
        }
    }
}
