﻿using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Core.Game.Interface.Data.Brand>
    {
        public BrandMap(string schema)
        {
            ToTable("Brands", schema);
        }
    }
}
