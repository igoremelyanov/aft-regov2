using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class RoundMap : EntityTypeConfiguration<Round>
    {
        public RoundMap(string schema)
        {
            ToTable("Rounds", schema);
            HasKey(x => x.Id);
            HasMany(x => x.GameActions);

            Property(x => x.ExternalRoundId)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ExternalBetId") { IsUnique = false }));

            HasRequired(o => o.Game)
                .WithMany()
                .HasForeignKey(o => o.GameId);
        }
    }
}
