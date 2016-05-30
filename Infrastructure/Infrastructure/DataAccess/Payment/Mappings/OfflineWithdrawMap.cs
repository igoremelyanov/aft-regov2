﻿using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class OfflineWithdrawMap : EntityTypeConfiguration<OfflineWithdraw>
    {
        public OfflineWithdrawMap()
        {
            ToTable("OfflineWithdraws", Configuration.Schema);
            HasKey(p => p.Id);
            HasRequired(p => p.PlayerBankAccount).WithMany().WillCascadeOnDelete(false);

            //Specifies the one-to-many relation between OfflineWithdraw table and OfflineWithdrawalHistory table.
            HasMany(s => s.OfflineWithdrawalHistory)
                .WithRequired(m => m.OfflineWithdraw)
                .HasForeignKey(k => k.OfflineWithdrawalId);

            Property(p => p.TransactionNumber);
            Property(p => p.Amount);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Remarks);
        }
    }
}
