using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class MatchingCriteriaMap : EntityTypeConfiguration<MatchingCriteria>
    {
        public MatchingCriteriaMap()
        {
            ToTable("MatchingCriterias", Configuration.Schema);
        }
    }
}
