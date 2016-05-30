using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class OnlineDepositMap : EntityTypeConfiguration<OnlineDeposit>
    {
        public OnlineDepositMap()
        {
            ToTable("OnlineDeposits", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(p => p.Player).WithMany().WillCascadeOnDelete(false);
            HasRequired(p => p.Brand).WithMany().WillCascadeOnDelete(false);

            Property(e => e.TransactionNumber)
                .HasMaxLength(50)
                .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("idx_TransactionNumber", 1) { IsUnique = true }));
        }
    }
}
