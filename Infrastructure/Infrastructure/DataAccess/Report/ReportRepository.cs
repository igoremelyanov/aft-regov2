using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Data.Brand;

namespace AFT.RegoV2.Infrastructure.DataAccess.Report
{
    public class ReportRepository : DbContext, IReportRepository
    {
        public const string Schema = "report";

        static ReportRepository()
        {
            Database.SetInitializer(new ReportRepositoryInitializer());
        }

        public ReportRepository()
            : base("name=Default")
        {
        }

        public void Initialize()
        {
            Database.Initialize(false);
        }

        // Admin
        public IDbSet<AdminActivityLog> AdminActivityLog { get; set; }
        public IDbSet<AdminAuthenticationLog> AdminAuthenticationLog { get; set; }
        public IDbSet<MemberAuthenticationLog> MemberAuthenticationLog { get; set; }

        // Player Reports
        public IDbSet<PlayerRecord> PlayerRecords { get; set; }
        public IDbSet<PlayerBetHistoryRecord> PlayerBetHistoryRecords { get; set; }

        // Payment Reports
        public IDbSet<DepositRecord> DepositRecords { get; set; }

        // Brand Reports
        public IDbSet<BrandRecord> BrandRecords { get; set; }
        public IDbSet<LicenseeRecord> LicenseeRecords { get; set; }
        public IDbSet<LanguageRecord> LanguageRecords { get; set; }
        public IDbSet<VipLevelRecord> VipLevelRecords { get; set; }

        // Transaction Reports
        public IDbSet<PlayerTransactionRecord> PlayerTransactionRecords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Admin
            modelBuilder.Entity<AdminActivityLog>().ToTable("AdminActivityLogs", Schema);
            modelBuilder.Entity<AdminAuthenticationLog>().ToTable("AdminAuthenticationLog", Schema);
            modelBuilder.Entity<MemberAuthenticationLog>().ToTable("MemberAuthenticationLog", Schema);

            // Player Reports
            modelBuilder.Entity<PlayerRecord>().ToTable("Players", Schema);
            modelBuilder.Entity<PlayerBetHistoryRecord>().ToTable("Bets", Schema);

            // Payment Reports
            modelBuilder.Entity<DepositRecord>().ToTable("Deposits", Schema);

            // Transaction Reports
            modelBuilder.Entity<PlayerTransactionRecord>().ToTable("PlayerTransactions", Schema);

            // Brand Reports
            modelBuilder.Entity<BrandRecord>().ToTable("Brands", Schema);
            modelBuilder.Entity<LicenseeRecord>().ToTable("Licensees", Schema);
            modelBuilder.Entity<LanguageRecord>().ToTable("Languages", Schema);
            modelBuilder.Entity<VipLevelRecord>().ToTable("VipLevels", Schema);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }
    }
}
