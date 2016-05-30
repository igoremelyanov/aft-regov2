﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class BrandGameProviderConfigurationMap : EntityTypeConfiguration<BrandGameProviderConfiguration>
    {
        public BrandGameProviderConfigurationMap(string schema)
        {
            ToTable("BrandGameProviderConfigurations", schema);

            HasKey(x => x.Id);

            Property(e => e.BrandId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_BrandGameProviderConfigurations_BrandId_GameProviderId", 1)
                        {
                            IsUnique = true
                        }));

            Property(e => e.GameProviderId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_BrandGameProviderConfigurations_BrandId_GameProviderId", 2)
                        {
                            IsUnique = true
                        }));

        }
    }
}
