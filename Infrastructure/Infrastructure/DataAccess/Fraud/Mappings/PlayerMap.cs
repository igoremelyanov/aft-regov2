using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class PlayerMap : EntityTypeConfiguration<Core.Fraud.Data.Player>
    {
        public PlayerMap()
        {
            ToTable("Players", Configuration.Schema);
            HasKey(x => x.Id);
        }
    }
}
