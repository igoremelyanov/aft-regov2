using AFT.RegoV2.Core.Game.Interface.Data;
using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class GameProviderMap : EntityTypeConfiguration<GameProvider>
    {
        public GameProviderMap(string schema)
        {
            ToTable("GameProviders", schema);

            HasKey(x => x.Id);
            HasMany(x => x.GameProviderConfigurations);
            HasMany(x => x.GameProviderCurrencies);
        }
    }
}
