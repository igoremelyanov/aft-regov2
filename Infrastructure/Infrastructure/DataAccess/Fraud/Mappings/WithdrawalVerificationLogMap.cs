using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class WithdrawalVerificationLogMap : EntityTypeConfiguration<WithdrawalVerificationLog>
    {
        public WithdrawalVerificationLogMap()
        {
            ToTable("WithdrawalVerificationLog", Configuration.Schema);
            HasKey(x => x.Id);
        }
    }
}
