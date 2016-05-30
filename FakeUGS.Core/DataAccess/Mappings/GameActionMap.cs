using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class GameActionMap : EntityTypeConfiguration<GameAction>
    {
        public GameActionMap(string schema)
        {
            ToTable("GameActions", schema);
            HasKey(x => x.Id);

            Property(x => x.ExternalTransactionId)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ExternalTransactionId") { IsUnique = false }));

            HasRequired(x => x.Round)
                .WithMany(x => x.GameActions)
                .Map(x => x.MapKey("RoundId"));
        }
    }
}
