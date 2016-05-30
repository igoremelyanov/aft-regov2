using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class GameGroupGameMap : EntityTypeConfiguration<Core.Game.Interface.Data.GameGroupGame>
    {
        public GameGroupGameMap(string schema)
        {
            ToTable("GameGroupGames", schema);

            HasKey(x => x.Id);

            Property(e => e.GameId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_GameGroupGames_GameId_GameGroupId", 1)
                        {
                            IsUnique = true
                        }));

            Property(e => e.GameGroupId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_GameGroupGames_GameId_GameGroupId", 2)
                        {
                            IsUnique = true
                        }));

        }
    }
}
