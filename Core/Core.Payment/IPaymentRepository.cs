using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Core.Payment
{
    public interface IPaymentRepository
    {
        IDbSet<PaymentLevel> PaymentLevels { get; }
        IDbSet<PaymentSettings> PaymentSettings { get; }
        IDbSet<OfflineDeposit> OfflineDeposits { get; }
        IDbSet<OfflineWithdraw> OfflineWithdraws { get; }
        IDbSet<Bank> Banks { get; }
        IDbSet<BankAccount> BankAccounts { get; }
        IDbSet<BankAccountType> BankAccountTypes { get; }
        IDbSet<PlayerBankAccount> PlayerBankAccounts { get; }
        IDbSet<AFT.RegoV2.Core.Payment.Data.Brand> Brands { get; }
        IDbSet<Licensee> Licensees { get; }
        IDbSet<VipLevel> VipLevels { get; }
        IDbSet<AFT.RegoV2.Core.Payment.Data.Player> Players { get; }
        IDbSet<PlayerPaymentLevel> PlayerPaymentLevels { get; }
        IDbSet<TransferSettings> TransferSettings { get; }
        IDbSet<TransferFund> TransferFunds { get; }
        IDbSet<OnlineDeposit> OnlineDeposits { get; }
        IDbSet<WithdrawalLock> WithdrawalLocks { get; }
        /// <summary>
        /// Deposit collection stores both online and offline deposits.
        /// we can query online and offline deposits from this collection.
        /// </summary>
        IDbSet<Deposit> Deposits { get; }
        int SaveChanges();
        Entities.OfflineDeposit GetDepositById(Guid id);
        Entities.BankAccount GetBankAccount(Guid id);

        IDbSet<Currency> Currencies { get; }
        IDbSet<CurrencyExchange> CurrencyExchanges { get; }
        IDbSet<BrandCurrency> BrandCurrencies { get; }
        IDbSet<Country> Countries { get; }
        IDbSet<OfflineWithdrawalHistory> OfflineWithdrawalHistories { get; }
        IDbSet<PaymentGatewaySettings> PaymentGatewaySettings { get; set; }
        void LockOnlineDeposit(string transactionNumber);
        void LockOnlineDeposit(Guid id);
    }
}