using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class GameProviderConfigurationsMap : EntityTypeConfiguration<GameProviderConfiguration>
    {
        public GameProviderConfigurationsMap(string schema)
        {
            ToTable("GameProviderConfigurations", schema);

            HasKey(x => x.Id);
            HasRequired(o => o.GameProvider);
        }
    }
}
