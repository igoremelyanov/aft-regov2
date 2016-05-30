using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Mappings
{
    public class BankAccountMap : EntityTypeConfiguration<BankAccount>
    {
        public BankAccountMap()
        {
            HasKey(x => x.Id);

            HasRequired(o => o.Bank)
                .WithMany()
                .HasForeignKey(o => o.BankId);
        }
    }
}
