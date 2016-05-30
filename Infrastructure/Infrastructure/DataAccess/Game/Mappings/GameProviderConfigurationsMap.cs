using AFT.RegoV2.Core.Game.Interface.Data;
using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
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
