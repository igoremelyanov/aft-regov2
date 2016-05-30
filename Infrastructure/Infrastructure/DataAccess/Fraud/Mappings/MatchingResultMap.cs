using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class MatchingResultMap : EntityTypeConfiguration<Core.Fraud.Data.MatchingResult>
    {
        public MatchingResultMap()
        {
            ToTable("MatchingResults", Configuration.Schema);
        }
    }
}
