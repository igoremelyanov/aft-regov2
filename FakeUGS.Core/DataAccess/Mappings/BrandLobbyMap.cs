using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class BrandLobbyMap : EntityTypeConfiguration<BrandLobby>
    {
        public BrandLobbyMap(string schema)
        {
            ToTable("BrandLobbies", schema);

            HasKey(x => x.Id);

            Property(e => e.BrandId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_BrandLobbies_BrandId_LobbyId", 1)
                        {
                            IsUnique = true
                        }));

            Property(e => e.LobbyId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_BrandLobbies_BrandId_LobbyId", 2)
                        {
                            IsUnique = true
                        }));

        }
    }
}
