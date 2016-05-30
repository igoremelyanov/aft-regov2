using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakePaymentRepository : PaymentRepository, IPaymentRepository
    {
        private readonly FakeDbSet<PaymentLevel> _paymentLevels = new FakeDbSet<PaymentLevel>();
        private readonly FakeDbSet<PaymentSettings> _paymentSettings = new FakeDbSet<PaymentSettings>();
        private readonly FakeDbSet<TransferSettings> _transferSettings = new FakeDbSet<TransferSettings>();
        private readonly FakeDbSet<TransferFund> _transferFund = new FakeDbSet<TransferFund>();
        private readonly FakeDbSet<OfflineDeposit> _offlineDeposits = new FakeDbSet<OfflineDeposit>();
        private readonly FakeDbSet<OfflineWithdraw> _offlineWithdraw = new FakeDbSet<OfflineWithdraw>();
        private readonly FakeDbSet<Bank> _banks = new FakeDbSet<Bank>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<Licensee> _licensees = new FakeDbSet<Licensee>();
        private readonly FakeDbSet<BrandCurrency> _brandCurrencies = new FakeDbSet<BrandCurrency>();
        private readonly FakeDbSet<CurrencyExchange> _currencyExchanges = new FakeDbSet<CurrencyExchange>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<BankAccountType> _bankAccountTypes = new FakeDbSet<BankAccountType>();
        private readonly FakeDbSet<BankAccount> _bankAccount = new FakeDbSet<BankAccount>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<Currency> _currencies = new FakeDbSet<Currency>();
        private readonly FakeDbSet<PlayerPaymentLevel> _playerPaymentLevels = new FakeDbSet<PlayerPaymentLevel>();
        private readonly FakeDbSet<PlayerBankAccount> _playerBankAccounts = new FakeDbSet<PlayerBankAccount>();
        private readonly FakeDbSet<Country> _countries = new FakeDbSet<Country>();
        private readonly FakeDbSet<OfflineWithdrawalHistory>  _offlineWithdrawalHistories= new FakeDbSet<OfflineWithdrawalHistory>();
        private readonly FakeDbSet<OnlineDeposit> _onlineDeposits = new FakeDbSet<OnlineDeposit>();
        private readonly FakeDbSet<PaymentGatewaySettings> _paymentGatewaySettings = new FakeDbSet<PaymentGatewaySettings>();
        private readonly FakeDbSet<Deposit> _deposits = new FakeDbSet<Deposit>();
        private readonly FakeDbSet<WithdrawalLock> _withdrawalLocks = new FakeDbSet<WithdrawalLock>();
        public override IDbSet<PaymentLevel> PaymentLevels { get { return _paymentLevels; } }
        public override IDbSet<PaymentSettings> PaymentSettings { get { return _paymentSettings; } }
        public override IDbSet<TransferSettings> TransferSettings { get { return _transferSettings; } }
        public override IDbSet<TransferFund> TransferFunds { get { return _transferFund; } }
        public override IDbSet<OfflineDeposit> OfflineDeposits { get { return _offlineDeposits; } }
        public override IDbSet<OfflineWithdraw> OfflineWithdraws { get { return _offlineWithdraw; } }
        public override IDbSet<Bank> Banks { get { return _banks; } }
        public override IDbSet<Brand> Brands { get { return _brands; } }
        public override IDbSet<Licensee> Licensees { get { return _licensees; } }
        public override IDbSet<BrandCurrency> BrandCurrencies { get { return _brandCurrencies; } }
        public override IDbSet<CurrencyExchange> CurrencyExchanges { get { return _currencyExchanges; } }
        public override IDbSet<VipLevel> VipLevels { get { return _vipLevels; } }
        public override IDbSet<BankAccountType> BankAccountTypes { get { return _bankAccountTypes; } }
        public override IDbSet<BankAccount> BankAccounts { get { return _bankAccount; } }
        public override IDbSet<Player> Players { get { return _players; } }
        public override IDbSet<Currency> Currencies { get { return _currencies; } }
        public override IDbSet<PlayerPaymentLevel> PlayerPaymentLevels { get { return _playerPaymentLevels; } }
        public override IDbSet<PlayerBankAccount> PlayerBankAccounts { get { return _playerBankAccounts; } }
        public override IDbSet<Country> Countries { get { return _countries; } }
        public override IDbSet<OfflineWithdrawalHistory> OfflineWithdrawalHistories { get { return _offlineWithdrawalHistories; } }
        public override IDbSet<OnlineDeposit> OnlineDeposits { get { return _onlineDeposits; } }
        public override IDbSet<PaymentGatewaySettings> PaymentGatewaySettings { get { return _paymentGatewaySettings; } }
        public override IDbSet<WithdrawalLock> WithdrawalLocks { get { return _withdrawalLocks; } }
        public override IDbSet<Deposit> Deposits { get { return _deposits; } }
        public new OfflineDeposit GetDepositById(Guid id)
        {
            throw new NotImplementedException();
        }

        public new BankAccount GetBankAccount(Guid id)
        {
            throw new NotImplementedException();
        }

        public new int SaveChanges()
        {
            if (!_offlineDeposits.AllElementsAreUnique())
            {
                throw new RegoException("OfflineDeposits with duplicate Ids were found");
            }

            foreach (var player in _players)
            {
                if (player.Brand == null)
                    player.Brand = _brands.Single(x => x.Id == player.BrandId);
            }

            foreach (var bank in _banks)
            {
                if (bank.Brand == null)
                    bank.Brand = _brands.Single(x => x.Id == bank.BrandId);
            }

            foreach (var paymentLevel in _paymentLevels)
            {
                if (paymentLevel.Brand == null)
                    paymentLevel.Brand = _brands.Single(x => x.Id == paymentLevel.BrandId);
            }

            return 0;
        }

        public new void LockOnlineDeposit(string transactionNumber)
        {
            
        }

        public new void LockOnlineDeposit(Guid id)
        {

        }
    }
}