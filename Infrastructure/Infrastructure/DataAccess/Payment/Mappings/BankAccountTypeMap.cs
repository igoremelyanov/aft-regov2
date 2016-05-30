using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BankAccountTypeMap : EntityTypeConfiguration<BankAccountType>
    {
        public BankAccountTypeMap()
        {
            ToTable("BankAccountTypes", Configuration.Schema);
        }
    }
}
