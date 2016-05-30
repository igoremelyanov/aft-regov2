using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class BetLimitMap : EntityTypeConfiguration<GameProviderBetLimit>
    {
        public BetLimitMap(string schema)
        {
            ToTable("BetLimits", schema);
        }
    }
}