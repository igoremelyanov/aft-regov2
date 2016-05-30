using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class VipLevelMap : EntityTypeConfiguration<Core.Messaging.Data.VipLevel>
    {
        public VipLevelMap(string schema)
        {
            ToTable("VipLevels", schema);
        }
    }
}