using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class PlayerMap : EntityTypeConfiguration<Player>
    {
        public PlayerMap(string schema)
        {
            ToTable("Players", schema);
        }
    }
}
