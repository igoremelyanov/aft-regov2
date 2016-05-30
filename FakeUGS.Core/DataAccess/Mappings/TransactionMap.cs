using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class TransactionMap : EntityTypeConfiguration<Transaction>
    {
        public TransactionMap(string schema)
        {
            ToTable("WalletTransactions", schema);

            Property(x => x.RoundId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BetId") { IsUnique = false }));
        }
    }
}
