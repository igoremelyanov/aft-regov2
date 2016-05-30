using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class DepositMap : EntityTypeConfiguration<Deposit>
    {
        public DepositMap()
        {
            ToTable("Deposits", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(p => p.Player).WithMany().WillCascadeOnDelete(false);
            HasRequired(p => p.Brand).WithMany().WillCascadeOnDelete(false);
            HasOptional(p => p.BankAccount).WithMany().WillCascadeOnDelete(false);
        }
    }
}
