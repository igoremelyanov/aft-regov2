using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Mappings
{
    public class PlayerBetStatisticsMap : EntityTypeConfiguration<PlayerBetStatistics>
    {
        public PlayerBetStatisticsMap()
        {
            HasKey(p => p.PlayerId);
            Property(p => p.TotalWon);
            Property(p => p.TotalLoss);
            Property(p => p.TotlAdjusted);
        }
    }
}