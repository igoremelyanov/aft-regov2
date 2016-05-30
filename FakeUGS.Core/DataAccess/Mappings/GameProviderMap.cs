using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
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
