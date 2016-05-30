using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class PlayerMap : EntityTypeConfiguration<Core.Game.Interface.Data.Player>
    {
        public PlayerMap(string schema)
        {
            ToTable("Players", schema);
        }
    }
}
