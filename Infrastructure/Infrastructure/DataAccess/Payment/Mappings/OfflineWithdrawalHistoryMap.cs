using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class OfflineWithdrawalHistoryMap : EntityTypeConfiguration<Core.Payment.Data.OfflineWithdrawalHistory>
    {
        public OfflineWithdrawalHistoryMap()
        {
            ToTable("OfflineWithdrawalHistory", Configuration.Schema);
            HasKey(x => x.Id);
        }
    }
}
