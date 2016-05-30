using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class WithdrawalLockMap : EntityTypeConfiguration<WithdrawalLock>
    {
        public WithdrawalLockMap()
        {
            ToTable("WithdrawalLocks", Configuration.Schema);

            Property(x => x.PlayerId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlayerId") { IsUnique = false }));
        }
    }
}
