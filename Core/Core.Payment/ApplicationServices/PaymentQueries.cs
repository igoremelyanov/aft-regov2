using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Shared.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using Microsoft.Practices.Unity;
using Player = AFT.RegoV2.Core.Payment.Data.Player;

namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class PaymentQueries : MarshalByRefObject, IPaymentQueries
    {
        private readonly IPaymentRepository _repository;
        private readonly IPlayerRepository _playerRepository;

        private readonly IPlayerQueries _playerQueries;

        private readonly BrandQueries _brandQueries;
        private readonly IUnityContainer _container;

        static PaymentQueries()
        {
            MapperConfig.CreateMap();
        }

        public PaymentQueries(
            IPaymentRepository repository,
            IPlayerRepository playerRepository,
            IPlayerQueries playerQueries,
            BrandQueries brandQueries,
            IUnityContainer container)
        {
            _repository = repository;
            _playerRepository = playerRepository;
            _playerQueries = playerQueries;
            _brandQueries = brandQueries;
            _container = container;
        }

        public OfflineDeposit GetDepositById(Guid id)
        {
            var offlineDeposit = GetOfflineDeposit(id);

            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit with Id {0} was not found", id));
            }
            return Mapper.Map<OfflineDeposit>(offlineDeposit);
        }

        public decimal CalculateFeeForDeposit(Guid depositId, decimal actualAmount)
        {
            var offlineDeposit = _repository.OfflineDeposits
                .Single(o => o.Id == depositId);

            var playerPaymentLevel = _repository.PlayerPaymentLevels
                .Include(o => o.PaymentLevel)
                .SingleOrDefault(o => o.PlayerId == offlineDeposit.PlayerId);

            if (playerPaymentLevel == null)
                return 0M;

            var paymentLevel = playerPaymentLevel.PaymentLevel;

            var preCalculatedFee = actualAmount * paymentLevel.BankFeeRatio / 100;

            var calculatedFee = preCalculatedFee > paymentLevel.MaxBankFee
                ? paymentLevel.MaxBankFee
                : preCalculatedFee;

            return calculatedFee;
        }

        private AFT.RegoV2.Core.Payment.Data.OfflineDeposit GetOfflineDeposit(Guid id)
        {
            var offlineDeposit = _repository.OfflineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount.Bank)
                .SingleOrDefault(x => x.Id == id);

            return offlineDeposit;
        }

        public IEnumerable<BankAccount> GetBankAccountsForAdminOfflineDepositRequest(Guid playerId)
        {
            return GetBankAccountsForOfflineDeposit(playerId);
        }

        public Dictionary<Guid, string> GetBankAccountsForOfflineDepositRequest(Guid playerId)
        {
            var bankAccounts = GetBankAccountsForOfflineDeposit(playerId);
            if (bankAccounts == null || !bankAccounts.Any())
                return new Dictionary<Guid, string>();

            return bankAccounts.ToDictionary(
                    account => account.Id,
                    account => String.Format("{0} / {1}", account.Bank.BankName, account.AccountName));
        }

        public IEnumerable<BankAccount> GetBankAccountsForOfflineDeposit(Guid playerId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            if (player == null)
            {
                throw new ArgumentException(@"Player was not found", "playerId");
            }

            var paymentLevel = _repository.PlayerPaymentLevels
                .Include(x => x.PaymentLevel.BankAccounts.Select(s => s.Bank))
                .Single(l => l.PlayerId == playerId)
                .PaymentLevel;

            if (!paymentLevel.EnableOfflineDeposit)
            {
                return new BankAccount[] { };
            }

            var bankAccounts = paymentLevel.BankAccounts.Where(x => x.Status == BankAccountStatus.Active);

            return Mapper.Map<IEnumerable<BankAccount>>(bankAccounts);
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositRequests)]
        public OfflineDeposit GetDepositByIdForViewRequest(OfflineDepositId id)
        {
            return GetDepositById(id);
        }

        [Permission(Permissions.Confirm, Module = Modules.OfflineDepositConfirmation)]
        public OfflineDeposit GetDepositByIdForConfirmation(OfflineDepositId id)
        {
            return GetDepositById(id);
        }

        public OfflineWithdraw GetWithdrawById(Guid id)
        {
            var offlineWithdraw = _repository.OfflineWithdraws
                .Include(x => x.PlayerBankAccount.Player)
                .Include(x => x.PlayerBankAccount.Bank.Brand)
                .FirstOrDefault(x => x.Id == id);
            if (offlineWithdraw == null)
            {
                throw new ArgumentException(string.Format("OfflineWithdraw with Id {0} was not found", id));
            }
            return Mapper.Map<OfflineWithdraw>(offlineWithdraw);
        }

        /// <summary>
        /// The purpose of that method is to generate "Verification queue" process result in a unified way through all the 
        /// verifications of the withdrawal menu.
        /// </summary>
        /// <param name="withdrawalId"></param>
        /// <returns></returns>
        public OfflineWithdrawalHistory GetVerificationQueueRecord(Guid withdrawalId)
        {
            var wdRecordHistory = _repository.OfflineWithdrawalHistories
                .Where(o => o.OfflineWithdrawalId == withdrawalId)
                .OrderByDescending(o => o.DateCreated).ToList();

            var match = wdRecordHistory.SkipWhile(rec => rec.Action != WithdrawalStatus.Verified &&
                                                         rec.Action != WithdrawalStatus.Documents &&
                                                         rec.Action != WithdrawalStatus.Investigation);

            if (!match.Any())
                return null;

            var observedHistoryRecord = match.FirstOrDefault();
            //TODO: This should be refactored and optimized. 
            //TODO: We can not be sure the structure will be always like this. Skip(1) must be considered.
            var previous = match.ToList().Skip(1).FirstOrDefault();

            //This would be the case where we had exemption for a withdrawal. In this case, the wd is verified on some of the first steps of the process.
            if (previous == null)
                return null;

            if (observedHistoryRecord.Action == WithdrawalStatus.Verified)
                return previous.Action == WithdrawalStatus.Reverted
                    ? Mapper.Map<OfflineWithdrawalHistory>(observedHistoryRecord)
                    : (previous.Action == WithdrawalStatus.Documents ||
                       previous.Action == WithdrawalStatus.Investigation)
                        ? Mapper.Map<OfflineWithdrawalHistory>(previous)
                        : null;
            
            return Mapper.Map<OfflineWithdrawalHistory>(observedHistoryRecord);
        }

        /// <summary>
        /// The purpose of that method is to generate "On hold queue" process result in a unified way through all the 
        /// verifications of the withdrawal menu.
        /// </summary>
        /// <param name="withdrawalId"></param>
        /// <returns></returns>
        public OfflineWithdrawalHistory GetOnHoldQueueRecord(Guid withdrawalId)
        {
            var wdRecordHistory = _repository.OfflineWithdrawalHistories
                .Where(o => o.OfflineWithdrawalId == withdrawalId)
                .OrderByDescending(o => o.DateCreated).ToList();

            OfflineWithdrawalHistory match = null, beforeMatch = null;

            for (int i = 0; i < wdRecordHistory.Count - 2; i++)
            {
                if (wdRecordHistory[i].Action == WithdrawalStatus.Verified
                    && wdRecordHistory[i + 1].Action != WithdrawalStatus.Reverted)
                {
                    match = Mapper.Map<OfflineWithdrawalHistory>( wdRecordHistory[i]);
                    beforeMatch =Mapper.Map<OfflineWithdrawalHistory>( wdRecordHistory[i + 1]);

                    break;
                }
            }

            return beforeMatch != null ? match : null;
        }

        public Bank GetBank(Guid id)
        {
            var bank = _repository.Banks
                .Include(x => x.Brand)
                .Include(x => x.Country)
                .Single(x => x.Id == id);

            return Mapper.Map<Bank>(bank);
        }

        public List<Bank> GetBanksByBrand(Guid brandId)
        {
            var query = _repository.Banks.Where(x => x.BrandId == brandId).ToList();
            return Mapper.Map<List<Bank>>(query);
        }

        public List<BankAccount> GetBankAccounts(Guid brandId, string currencyCode)
        {
            var query= _repository
                .BankAccounts
                .Include(o => o.Bank)
                .Where(x => x.Bank.BrandId == brandId && x.CurrencyCode == currencyCode)
                .ToList();
            return Mapper.Map<List<BankAccount>>(query);
        }

        public PaymentSettingTransferObj GetPaymentSettingById(Guid id)
        {
            var paymentSettings = GetPaymentSettings(id);

            var obj = new PaymentSettingTransferObj
            {
                Brand = new
                {
                    paymentSettings.Brand.Id,
                    paymentSettings.Brand.Name,
                    Licensee = new
                    {
                        id = paymentSettings.Brand.LicenseeId
                    }
                },
                PaymentType = paymentSettings.PaymentType.ToString(),
                CurrencyCode = paymentSettings.CurrencyCode,
                VipLevel = _playerQueries.VipLevels.Single(x => x.Id == new Guid(paymentSettings.VipLevel)).Name,
                Id = paymentSettings.Id,
                PaymentMethod = paymentSettings.PaymentMethod,
                MinAmountPerTransaction = paymentSettings.MinAmountPerTransaction,
                MaxAmountPerTransaction = paymentSettings.MaxAmountPerTransaction,
                MaxAmountPerDay = paymentSettings.MaxAmountPerDay,
                MaxTransactionPerDay = paymentSettings.MaxTransactionPerDay,
                MaxTransactionPerWeek = paymentSettings.MaxTransactionPerWeek,
                MaxTransactionPerMonth = paymentSettings.MaxTransactionPerMonth,
            };

            return obj;
        }

        public IQueryable<BankAccount> GetBankAccounts()
        {
            return _repository.BankAccounts
                .Include(o => o.AccountType)
                .Include(o => o.Bank)
                .AsNoTracking().Project().To<BankAccount>(null, dest => dest.PaymentLevels);
        }

        public IQueryable<Bank> GetBanks()
        {
            return _repository.Banks.AsNoTracking().Project().To<Bank>();
        }

        private AFT.RegoV2.Core.Payment.Data.PaymentSettings GetPaymentSettings(Guid id)
        {
            var paymentSettings = _repository.PaymentSettings
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == id);

            if (paymentSettings == null)
            {
                throw new ArgumentException("Payment settings not found");
            }
            return paymentSettings;
        }

        [Permission(Permissions.View, Module = Modules.PaymentSettings)]
        public IQueryable<PaymentSettings> GetPaymentSettings()
        {
            return _repository.PaymentSettings
                .Include(x => x.Brand)
                .AsNoTracking().Project().To<PaymentSettings>();
        }

        public IEnumerable<PaymentLevelDTO> GetPaymentLevels(Guid brandId)
        {
            var levels = from x in _repository.PaymentLevels.Where(x=>x.BrandId==brandId).ToList()
                         let isDefault = _brandQueries.GetDefaultPaymentLevelId(x.BrandId, x.CurrencyCode) == x.Id
                         select new PaymentLevelDTO
                         {
                             Id = x.Id,
                             Name = x.Name,
                             Code = x.Code,
                             BrandId = x.BrandId,
                             CurrencyCode = x.CurrencyCode,
                             IsDefault = isDefault
                         };

            return levels;
        }

        [Permission(Permissions.View, Module = Modules.PaymentLevelManager)]
        public IQueryable<PaymentLevel> GetPaymentLevelsAsQueryable()
        {
            return _repository.PaymentLevels.AsNoTracking().Project().To<PaymentLevel>();
        }

        public PaymentSettingDTO GetPaymentSetting(Guid brandId, string currencyCode, VipLevelViewModel vipLevel, PaymentType type)
        {
            if (vipLevel == null)
                return null;

            var paymentSetting = _repository
                .PaymentSettings
                .FirstOrDefault(
                    ps => ps.BrandId == brandId &&
                    ps.CurrencyCode == currencyCode &&
                    ps.VipLevel == vipLevel.Id.ToString()
                    && ps.PaymentType == type);

            if (paymentSetting == null)
                return null;

            return new PaymentSettingDTO
            {
                CurrencyCode = paymentSetting.CurrencyCode,
                VipLevel = paymentSetting.VipLevel,
                MinAmountPerTransaction = paymentSetting.MinAmountPerTransaction,
                MaxAmountPerTransaction = paymentSetting.MaxAmountPerTransaction,
                MaxAmountPerDay = paymentSetting.MaxAmountPerDay,
                MaxTransactionPerDay = paymentSetting.MaxTransactionPerDay,
                MaxTransactionPerWeek = paymentSetting.MaxTransactionPerWeek,
                MaxTransactionPerMonth = paymentSetting.MaxTransactionPerMonth
            };
        }

        private AFT.RegoV2.Core.Payment.Data.Brand GetBrand(Guid id)
        {
            return _repository.Brands
                .FirstOrDefault(x => x.Id == id);
        }

        public AFT.RegoV2.Core.Payment.Interface.Data.VipLevel GetVipLevel(Guid id)
        {
            var query = _repository.VipLevels
                .FirstOrDefault(x => x.Id == id);

            return Mapper.Map<AFT.RegoV2.Core.Payment.Interface.Data.VipLevel>(query);
        }

        public IQueryable<AFT.RegoV2.Core.Payment.Interface.Data.VipLevel> VipLevels()
        {
            return _repository.VipLevels.AsNoTracking().Project().To<AFT.RegoV2.Core.Payment.Interface.Data.VipLevel>();
        }

        public TransferSettings GetTransferSettings(Guid id)
        {
            var transferSettings = _repository.TransferSettings
                .Include(x => x.Brand)
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.Id == id);

            if (transferSettings == null)
            {
                throw new ArgumentException("Transfer settings not found");
            }
            return Mapper.Map<TransferSettings>(transferSettings);
        }

        [Permission(Permissions.View, Module = Modules.TransferSettings)]
        public IQueryable<TransferSettings> GetTransferSettings()
        {
            return _repository.TransferSettings
                .Include(x => x.Brand)
                .Include(x => x.VipLevel)
                .AsNoTracking().Project().To<TransferSettings>();
        }

        public TransferSettingDTO GetTransferSetting(Guid brandId, string currencyCode, VipLevelViewModel vipLevel)
        {
            if (vipLevel == null)
                return null;

            var paymentSetting = _repository
                .TransferSettings
                .FirstOrDefault(ps => ps.BrandId == brandId && ps.CurrencyCode == currencyCode && ps.VipLevelId == vipLevel.Id);

            if (paymentSetting == null)
                return null;

            return new TransferSettingDTO
            {
                CurrencyCode = paymentSetting.CurrencyCode,
                VipLevel = paymentSetting.VipLevelId.ToString(),
                MinAmountPerTransaction = paymentSetting.MinAmountPerTransaction,
                MaxAmountPerTransaction = paymentSetting.MaxAmountPerTransaction,
                MaxAmountPerDay = paymentSetting.MaxAmountPerDay,
                MaxTransactionPerDay = paymentSetting.MaxTransactionPerDay,
                MaxTransactionPerWeek = paymentSetting.MaxTransactionPerWeek,
                MaxTransactionPerMonth = paymentSetting.MaxTransactionPerMonth
            };
        }

        public TransferSettingDTO GetTransferSetting(string walletId, TransferFundType transferFundType, bool enabled)
        {
            var transferSetting = _repository
                .TransferSettings
                .FirstOrDefault(
                        ts => ts.WalletId.Equals(walletId) &&
                        ts.TransferType == transferFundType &&
                        ts.Enabled == enabled);

            if (transferSetting == null)
                return null;

            return new TransferSettingDTO
            {
                CurrencyCode = transferSetting.CurrencyCode,
                VipLevel = transferSetting.VipLevelId.ToString(),
                MinAmountPerTransaction = transferSetting.MinAmountPerTransaction,
                MaxAmountPerTransaction = transferSetting.MaxAmountPerTransaction,
                MaxAmountPerDay = transferSetting.MaxAmountPerDay,
                MaxTransactionPerDay = transferSetting.MaxTransactionPerDay,
                MaxTransactionPerWeek = transferSetting.MaxTransactionPerWeek,
                MaxTransactionPerMonth = transferSetting.MaxTransactionPerMonth
            };
        }

        [Permission(Permissions.Create, Module = Modules.PlayerBankAccount)]
        public AFT.RegoV2.Core.Payment.Interface.Data.Player GetPlayerForNewBankAccount(AFT.RegoV2.Core.Payment.Interface.Data.PlayerId playerId)
        {
            return GetPlayerHelper(playerId, q => q.Include(x => x.CurrentBankAccount));
        }

        public Core.Payment.Interface.Data.Player GetPlayerWithBank(Guid playerId)
        {
            return GetPlayerHelper(playerId, q => q.Include(x => x.CurrentBankAccount.Bank));
        }

        public Core.Payment.Interface.Data.Player GetPlayer(Guid playerId)
        {
            return GetPlayerHelper(playerId, null);
        }

        private Core.Payment.Interface.Data.Player GetPlayerHelper(Guid playerId, Func<IQueryable<AFT.RegoV2.Core.Payment.Data.Player>, IQueryable<AFT.RegoV2.Core.Payment.Data.Player>> modifyQuery)
        {
            var queryable = _repository.Players.Include(x => x.CurrentBankAccount).AsQueryable();
            if (modifyQuery != null)
            {
                queryable = modifyQuery(queryable);
            }
            var playerData = queryable.SingleOrDefault(x => x.Id == playerId);
            if (playerData != null)
                return Mapper.DynamicMap<Core.Payment.Interface.Data.Player>(playerData);

            throw new ArgumentException("Player not found");
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositRequests)]
        public IQueryable<OfflineDeposit> GetOfflineDeposits()
        {
            return _repository.OfflineDeposits.AsNoTracking().Project().To<OfflineDeposit>();
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositConfirmation)]
        public IQueryable<OfflineDeposit> GetDepositsAsConfirmed()
        {
            return _repository.OfflineDeposits.AsNoTracking().Project().To<OfflineDeposit>();
        }

        [Permission(Permissions.View, Module = Modules.DepositVerification)]
        public IQueryable<OfflineDeposit> GetDepositsAsVerified()
        {
            return _repository.OfflineDeposits.AsNoTracking().Project().To<OfflineDeposit>();
        }

        [Permission(Permissions.View, Module = Modules.PlayerBankAccount)]
        public IQueryable<PlayerBankAccount> GetPlayerBankAccounts()
        {
            return _repository.PlayerBankAccounts
                .Include(x => x.Player.CurrentBankAccount)
                .Include(x => x.Bank)
                .AsNoTracking().Project().To<PlayerBankAccount>();
        }

        [Permission(Permissions.Update, Module = Modules.PlayerBankAccount)]
        public AFT.RegoV2.Core.Payment.Data.PlayerBankAccount GetPlayerBankAccountForEdit(PlayerBankAccountId id)
        {
            return _repository.PlayerBankAccounts
                .Include(x => x.Player.CurrentBankAccount)
                .Include(x => x.Bank)
                .SingleOrDefault(x => x.Id == id);
        }
        
        [Permission(Permissions.Update, Module = Modules.PlayerBankAccount)]
        public AFT.RegoV2.Core.Payment.Data.PlayerBankAccount GetPlayerBankAccountForSetCurrent(PlayerBankAccountId id)
        {
            return _repository.PlayerBankAccounts.Include(x => x.Player).SingleOrDefault(x => x.Id == id);
        }

        public IQueryable<PlayerPaymentLevel> GetPlayerPaymentLevels()
        {
            return _repository.PlayerPaymentLevels.AsNoTracking()
                .Project().To<PlayerPaymentLevel>();
        }

        public ValidationResult ValidatePlayerBankAccount(EditPlayerBankAccountData data)
        {
            return new AddPlayerBankAccountValidator(_repository, this).Validate(data);
        }

        public ValidationResult ValidateOfflineWithdrawalRequest(OfflineWithdrawRequest data)
        {
            var offlineWithdrawalRequest = new OfflineWithdrawalRequestValidator(_container,_repository);
            return offlineWithdrawalRequest.Validate(data);
        }

        public ValidationResult ValidateOfflineDepositRequest(OfflineDepositConfirm data)
        {
            var offlineWithdrawalRequest = new OfflineDepositConfirmValidator(_repository, _playerRepository, this);
            return offlineWithdrawalRequest.Validate(data);
        }

        public PaymentLevel GetPaymentLevel(Guid id)
        {
            var query = _repository.PaymentLevels
                .Include(l => l.Brand)
                .Include(l => l.BankAccounts)
                .SingleOrDefault(l => l.Id == id);
            return Mapper.Map<PaymentLevel>(query);
        }

        public BankAccount GetBankAccountForOfflineDeposit(Guid offlineDepositId)
        {
            var offlineDeposit = GetOfflineDeposits()
                .Single(o => o.Id == offlineDepositId);

            var bankAccountId = offlineDeposit.BankAccountId;

            var bankAccount = GetBankAccountsForOfflineDeposit(offlineDeposit.PlayerId)
                .SingleOrDefault(o => o.Id == bankAccountId);

            if (bankAccount == null)
                throw new RegoException("Offline deposit bank account is not enabled for player's payment level.");

            return bankAccount;
        }

        public IQueryable<OfflineWithdraw> GetOfflineWithdraws()
        {
            return _repository.OfflineWithdraws.AsNoTracking().Project().To<OfflineWithdraw>();
        }

        public List<OfflineWithdrawalHistory> WithdrawalHistories(Guid wdId)
        {
            var query = _repository.OfflineWithdrawalHistories
                .Where(historyElement => historyElement.OfflineWithdrawalId == wdId)
                .ToList();
            return Mapper.Map<List<OfflineWithdrawalHistory>>(query);
        }

        public PaymentSettings GetOnlinePaymentSettings(Guid brandId, PaymentType type, string vipLevel,
            string paymentGatewayName, string currencyCode)
        {
            var query = _repository.PaymentSettings.SingleOrDefault(x =>
                x.BrandId == brandId
                && x.PaymentType == type
                && x.VipLevel == vipLevel
                && x.PaymentGatewayMethod == PaymentMethod.Online
                && x.PaymentMethod.Equals(paymentGatewayName, StringComparison.InvariantCultureIgnoreCase)
                && x.CurrencyCode == currencyCode
                && x.Enabled == Status.Active);
            return Mapper.Map<PaymentSettings>(query);
        }

        public PaymentSettings GetOfflinePaymentSettings(Guid brandId, PaymentType type, string vipLevel, string currencyCode)
        {
            var query = _repository.PaymentSettings.SingleOrDefault(x =>
                x.BrandId == brandId
                && x.PaymentType == type
                && x.VipLevel == vipLevel
                && x.PaymentGatewayMethod == PaymentMethod.OfflineBank
                && x.CurrencyCode == currencyCode
                && x.Enabled == Status.Active);
            return Mapper.Map<PaymentSettings>(query);
        }

        public IQueryable<Core.Payment.Interface.Data.PaymentGatewaySettings> GetPaymentGatewaySettings()
        {
            return _repository.PaymentGatewaySettings
                .Include(x => x.Brand)
                .AsNoTracking().Project().To<Core.Payment.Interface.Data.PaymentGatewaySettings>(null, dest => dest.PaymentLevels);
        }

        public DateTimeOffset GetBrandDateTimeOffset(Guid brandId)
        {
            var brand = GetBrand(brandId);

            return AFT.RegoV2.Core.Common.Utils.DateTimeOffsetExtensions.ToBrandOffset(DateTimeOffset.Now, brand.TimezoneId);
        }

        public IQueryable<DepositDto> GetDeposits()
        {
            return _repository.Deposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount)
                .Select(obj => new DepositDto
                {
                    Id = obj.Id,
                    Licensee = obj.Licensee,
                    BrandId = obj.BrandId,
                    BrandName = obj.Brand.Name,
                    PlayerId = obj.PlayerId,
                    Username = obj.Player.Username,
                    FirstName = obj.Player.FirstName,
                    LastName = obj.Player.LastName,
                    ReferenceCode = obj.ReferenceCode,
                    PaymentMethod = obj.PaymentMethod,
                    CurrencyCode = obj.CurrencyCode,
                    Amount = obj.Amount,
                    UniqueDepositAmount = obj.UniqueDepositAmount,
                    Status = obj.Status,
                    DateSubmitted = obj.DateSubmitted,
                    SubmittedBy = obj.SubmittedBy,
                    DateVerified = obj.DateVerified,
                    VerifiedBy = obj.VerifiedBy,
                    DateApproved = obj.DateApproved,
                    ApprovedBy = obj.ApprovedBy,
                    DateRejected = obj.DateRejected,
                    RejectedBy = obj.RejectedBy,
                    DepositType = obj.DepositType,
                    BankAccountId = obj.BankAccount.AccountId,
                    BankName = obj.BankAccount.Bank.BankName,
                    BankProvince = obj.BankAccount.Province,
                    BankBranch = obj.BankAccount.Branch,
                    BankAccountNumber = obj.BankAccount.AccountName,
                    BankAccountName = obj.BankAccount.AccountNumber,
                    UnverifyReason = obj.UnverifyReason
                });
        }

        public IQueryable<DepositDto> GetDepositsAsQueryable()
        {
            return _repository.Deposits
                .Select(obj => new DepositDto
                {
                    Id = obj.Id,
                    Licensee = obj.Licensee,
                    BrandId = obj.BrandId,
                    PlayerId = obj.PlayerId,
                    ReferenceCode = obj.ReferenceCode,
                    PaymentMethod = obj.PaymentMethod,
                    CurrencyCode = obj.CurrencyCode,
                    Amount = obj.Amount,
                    UniqueDepositAmount = obj.UniqueDepositAmount,
                    Status = obj.Status,
                    DateSubmitted = obj.DateSubmitted,
                    SubmittedBy = obj.SubmittedBy,
                    DateVerified = obj.DateVerified,
                    VerifiedBy = obj.VerifiedBy,
                    DateApproved = obj.DateApproved,
                    ApprovedBy = obj.ApprovedBy,
                    DateRejected = obj.DateRejected,
                    RejectedBy = obj.RejectedBy,
                    DepositType = obj.DepositType
                });
        }

        public decimal GetWithdrawalLockBalance(Guid playerId)
        {
            return _repository.WithdrawalLocks
            	.Where(x => x.PlayerId == playerId && x.Status == Status.Active)
                .Sum(x => (decimal?)x.Amount) ?? 0;
        }

        public IQueryable<PlayerPaymentAmount> GetPlayerDepositAmount(GetPlayerPaymentAmountRequest request)
        {
            var query = _repository.Players
                .Include(x => x.Brand)
                .Include(x => x.PlayerPaymentLevel);

            query = GetPlayerFilterQueryable(request, query);

            var depositQuery = GetDepositFilterQueryable(request);

            var resultQuery =query.Select(obj => new PlayerPaymentAmount
                    {
                        PlayerId = obj.Id,
                        LicenseeName = obj.Brand.LicenseeName,
                        BrandId = obj.BrandId,
                        BrandName = obj.Brand.Name,
                        Username = obj.Username,
                        FullName = obj.FirstName+ " "+ obj.LastName,
                        PaymentLevelName = obj.PlayerPaymentLevel.PaymentLevel.Name,
                        PaymentLevelId = obj.PlayerPaymentLevel.PaymentLevel.Id,
                        Amount = depositQuery.Where(p=>p.PlayerId==obj.Id).Sum(x=>(decimal?)x.Amount)??0,
                        Currency = obj.CurrencyCode
                    });

            return resultQuery;
        }

        private IQueryable<Player> GetPlayerFilterQueryable(GetPlayerPaymentAmountRequest request, IQueryable<Player> query)
        {
            var now = DateTimeOffset.Now;
            Expression<Func<Player, bool>> conditions = x => false;
            Expression<Func<Player, bool>> expIsActive = x => x.IsActive == request.IsActive;
            Expression<Func<Player, bool>> expTimeOut = x => x.IsTimeOut && x.TimeOutEndDate.Value > now;
            Expression<Func<Player, bool>> expSelfExculde = x => x.IsSelfExclude && x.SelfExcludeEndDate > now;

            bool queryActive = request.IsActive != request.IsInactive;
            bool queryTimeOut = request.IsTimeOut;
            bool querySelfExcluded = request.IsSelfExcluded;
            bool queryAll = (request.IsActive == request.IsInactive) && (request.IsActive);

            if (false== queryAll &&( queryActive || queryTimeOut || querySelfExcluded))
            {
                if (queryActive)
                {
                    conditions = conditions.OrElse(expIsActive);
                }
                if (request.IsTimeOut)
                {
                    conditions = conditions.OrElse(expTimeOut);
                }
                if (request.IsSelfExcluded)
                {
                    conditions = conditions.OrElse(expSelfExculde);
                }

                //Expression<Func<Player, bool>> wherePredicate = Expression.Lambda<Func<Player, bool>>(conditions);

                query = query.Where(conditions);
            }
            return query;
        }

        

        public IQueryable<PlayerPaymentAmount> GetPlayerWithdrawalAmount(GetPlayerPaymentAmountRequest request)
        {
            var query = _repository.Players
                .Include(x => x.Brand)
                .Include(x => x.PlayerPaymentLevel);

            query = GetPlayerFilterQueryable(request, query);

            var withdrawalQuery = GetwithdrawalFilterQueryable(request);

            var resultQuery = query.Select(obj => new PlayerPaymentAmount
            {
                PlayerId = obj.Id,
                LicenseeName = obj.Brand.LicenseeName,
                BrandId = obj.BrandId,
                BrandName = obj.Brand.Name,
                Username = obj.Username,
                FullName = obj.FirstName + " " + obj.LastName,
                PaymentLevelName = obj.PlayerPaymentLevel.PaymentLevel.Name,
                PaymentLevelId = obj.PlayerPaymentLevel.PaymentLevel.Id,
                Amount = withdrawalQuery.Where(p => p.PlayerBankAccount.Player.Id == obj.Id).Sum(x => (decimal?)x.Amount) ?? 0,
                Currency = obj.CurrencyCode
            });

            return resultQuery;
        }

        private IQueryable<Core.Payment.Data.Deposit> GetDepositFilterQueryable(GetPlayerPaymentAmountRequest request)
        {
            var depositQuery = _repository.Deposits
                .Where(d => d.Status == OfflineDepositStatus.Approved.ToString());
            if (request.DateApprovedStart.HasValue)
                depositQuery = depositQuery.Where(d => d.DateApproved >= request.DateApprovedStart.Value);
            if (request.DateApprovedEnd.HasValue)
                depositQuery = depositQuery.Where(d => d.DateApproved < request.DateApprovedEnd.Value);
            if (request.PaymentMethods.Count > 0)
                depositQuery = depositQuery.Where(d => request.PaymentMethods.Contains(d.PaymentMethod));

            return depositQuery;
        }

        private IQueryable<Core.Payment.Data.OfflineWithdraw> GetwithdrawalFilterQueryable(GetPlayerPaymentAmountRequest request)
        {
            var withdrawalQuery = _repository.OfflineWithdraws
                .Where(d => d.Status == WithdrawalStatus.Approved);
            if (request.DateApprovedStart.HasValue)
                withdrawalQuery = withdrawalQuery.Where(d => d.Approved >= request.DateApprovedStart.Value);
            if (request.DateApprovedEnd.HasValue)
                withdrawalQuery = withdrawalQuery.Where(d => d.Approved< request.DateApprovedEnd.Value);

            return withdrawalQuery;
        }
        #region currency

        public IQueryable<Currency> GetCurrencies()
        {
            return _repository.Currencies.AsNoTracking().AsNoTracking().Project().To<Currency>();
        }

        public IQueryable<AFT.RegoV2.Core.Payment.Data.Currency> GetCurrencies(bool isActive)
        {
            //todo: currency: when conditions of not active currency appear
            //return _repository.Currencies.Where(c => c.Status == (isActive ? CurrencyStatus.Active : CurrencyStatus.Inactive)).AsNoTracking();
            return _repository.Currencies.AsNoTracking();
        }

        public Currency GetCurrency(string currencyCode)
        {
            return GetCurrencies().SingleOrDefault(x => x.Code == currencyCode);
        }

        #endregion currency

        #region currency exchange

        public IQueryable<CurrencyExchange> GetCurrencyExchanges()
        {
            return _repository.CurrencyExchanges.AsNoTracking().Project().To<CurrencyExchange>();
        }

        public IQueryable<CurrencyExchange> GetCurrencyExchangesbyBrand(Guid brandId)
        {
            return _repository.CurrencyExchanges
                .Include(x => x.Brand)
                .Include(x => x.CurrencyTo)
                .AsNoTracking().Project().To<CurrencyExchange>();
        }

        public CurrencyExchange GetCurrencyExchange(Guid brandId, string currencyCode)
        {
            var currencyExchange = _repository.CurrencyExchanges
                .Include(x => x.Brand)
                .Include(x => x.CurrencyTo)
                .SingleOrDefault(x => x.Brand.Id == brandId && x.CurrencyTo.Code == currencyCode);

            if (currencyExchange == null)
            {
                throw new RegoException("Currency exchange not found");
            }
            return Mapper.Map<CurrencyExchange>(currencyExchange);
        }

        public IQueryable<Licensee> GetLicensees()
        {
            return _repository.Licensees.AsNoTracking().Project().To<Licensee>();
        }

        public IQueryable<Core.Payment.Interface.Data.Brand> GetBrands()
        {
            return _repository.Brands.AsNoTracking().Project().To<Core.Payment.Interface.Data.Brand>();
        }

        public IQueryable<BrandCurrency> GetBrandCurrencies(Guid brandId)
        {
            return _repository.BrandCurrencies.Where(b => b.BrandId == brandId).AsNoTracking().Project().To<BrandCurrency>();
        }

        #endregion
    }
}
