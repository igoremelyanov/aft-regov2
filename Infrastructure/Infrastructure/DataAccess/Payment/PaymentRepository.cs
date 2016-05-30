using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.Data;

using AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository
{
    public class PaymentRepository : DbContext, IPaymentRepository
    {
        static PaymentRepository()
        {
            Database.SetInitializer(new PaymentRepositoryInitializer());
        }

        public PaymentRepository()
            : base("name=Default")
        {
        }

        public virtual IDbSet<PaymentLevel> PaymentLevels { get; set; }
        public virtual IDbSet<PaymentSettings> PaymentSettings { get; set; }
        public virtual IDbSet<OfflineDeposit> OfflineDeposits { get; set; }
        public virtual IDbSet<OfflineWithdraw> OfflineWithdraws { get; set; }
        public virtual IDbSet<Bank> Banks { get; set; }
        public virtual IDbSet<Core.Payment.Data.Brand> Brands { get; set; }
        public virtual IDbSet<Licensee> Licensees { get; set; }
        public virtual IDbSet<BrandCurrency> BrandCurrencies { get; set; }
        public virtual IDbSet<VipLevel> VipLevels { get; set; }
        public virtual IDbSet<BankAccountType> BankAccountTypes { get; set; }
        public virtual IDbSet<BankAccount> BankAccounts { get; set; }
        public virtual IDbSet<PlayerBankAccount> PlayerBankAccounts { get; set; }
        public virtual IDbSet<Core.Payment.Data.Player> Players { get; set; }
        public virtual IDbSet<PlayerPaymentLevel> PlayerPaymentLevels { get; set; }
        public virtual IDbSet<TransferSettings> TransferSettings { get; set; }
        public virtual IDbSet<TransferFund> TransferFunds { get; set; }
        public virtual IDbSet<Currency> Currencies { get; set; }
        public virtual IDbSet<CurrencyExchange> CurrencyExchanges { get; set; }
        public virtual IDbSet<Country> Countries { get; set; }
        public virtual IDbSet<OfflineWithdrawalHistory> OfflineWithdrawalHistories { get; set; }
        public virtual IDbSet<OnlineDeposit> OnlineDeposits { get; set; }
        public virtual IDbSet<PaymentGatewaySettings> PaymentGatewaySettings { get; set; }
        public virtual IDbSet<WithdrawalLock> WithdrawalLocks { get; set; }
        public virtual IDbSet<Deposit> Deposits { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new CountryMap());
            modelBuilder.Configurations.Add(new BankMap());
            modelBuilder.Configurations.Add(new BrandMap());
            modelBuilder.Configurations.Add(new LicenseeMap());
            modelBuilder.Configurations.Add(new BrandCurrencyMap());
            modelBuilder.Configurations.Add(new VipLevelMap());
            modelBuilder.Configurations.Add(new BankAccountTypeMap());
            modelBuilder.Configurations.Add(new BankAccountMap());
            modelBuilder.Configurations.Add(new PlayerBankAccountMap());
            modelBuilder.Configurations.Add(new OfflineDepositMap());
            modelBuilder.Configurations.Add(new OfflineWithdrawMap());
            modelBuilder.Configurations.Add(new PaymentLevelMap());
            modelBuilder.Configurations.Add(new PaymentSettingsMap());
            modelBuilder.Configurations.Add(new PlayerPaymentLevelMap());
            modelBuilder.Configurations.Add(new PlayerMap());
            modelBuilder.Configurations.Add(new TransferSettingsMap());
            modelBuilder.Configurations.Add(new TransferFundMap());
            modelBuilder.Configurations.Add(new CurrencyMap());
            modelBuilder.Configurations.Add(new CurrencyExchangeMap());
            modelBuilder.Configurations.Add(new OfflineWithdrawalHistoryMap());
            modelBuilder.Configurations.Add(new OnlineDepositMap());
            modelBuilder.Configurations.Add(new PaymentGatewaySettingsMap());
            modelBuilder.Configurations.Add(new WithdrawalLockMap());
            modelBuilder.Configurations.Add(new DepositMap());
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

        public Core.Payment.Entities.OfflineDeposit GetDepositById(Guid id)
        {
            OfflineDeposit offlineDeposit = OfflineDeposits
                .Include(p => p.BankAccount.Bank)
                .Include(p => p.Player)
                .Include(x => x.Player.Brand)
                .FirstOrDefault(x => x.Id == id);
            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit with Id {0} was not found", id));
            }
            return new Core.Payment.Entities.OfflineDeposit(offlineDeposit);
        }

        public Core.Payment.Entities.BankAccount GetBankAccount(Guid id)
        {
            var bankAccount = BankAccounts
                .Include(x => x.Bank)
                .Include(x => x.Bank.Brand)
                .Include(x => x.PaymentLevels)
                .Include(x => x.AccountType)
                .First(x => x.Id == id);

            if (bankAccount == null)
            {
                throw new ArgumentException("Bank not found");
            }
            return new Core.Payment.Entities.BankAccount(bankAccount);
        }
        
        public void LockOnlineDeposit(string transactionNumber)
        {
            Database.ExecuteSqlCommand(
                "SELECT Id FROM payment.OnlineDeposits WITH (ROWLOCK, UPDLOCK) WHERE TransactionNumber = @p0",
                transactionNumber);
        }

        public void LockOnlineDeposit(Guid id)
        {
            Database.ExecuteSqlCommand(
                "SELECT Id FROM payment.OnlineDeposits WITH (ROWLOCK, UPDLOCK) WHERE Id = @p0",
                id);
        }
    }
}
