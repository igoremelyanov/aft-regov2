using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation.Results;
using Player = AFT.RegoV2.Core.Payment.Interface.Data.Player;
using PlayerId = AFT.RegoV2.Core.Payment.Interface.Data.PlayerId;
using VipLevel = AFT.RegoV2.Core.Payment.Interface.Data.VipLevel;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentQueries : IBasePaymentQueries
    {        
        decimal CalculateFeeForDeposit(Guid depositId, decimal actualAmount);
        IEnumerable<BankAccount> GetBankAccountsForAdminOfflineDepositRequest(Guid playerId);
        Dictionary<Guid, string> GetBankAccountsForOfflineDepositRequest(Guid playerId);
        OfflineDeposit GetDepositById(Guid id);
        OfflineDeposit GetDepositByIdForViewRequest(OfflineDepositId id);
        OfflineDeposit GetDepositByIdForConfirmation(OfflineDepositId id);
        OfflineWithdraw GetWithdrawById(Guid id);
        OfflineWithdrawalHistory GetVerificationQueueRecord(Guid withdrawalId);
        OfflineWithdrawalHistory GetOnHoldQueueRecord(Guid withdrawalId);
        Bank GetBank(Guid id);
        List<Bank> GetBanksByBrand(Guid brandId);
        List<BankAccount> GetBankAccounts(Guid brandId, string currencyCode);
        PaymentSettingTransferObj GetPaymentSettingById(Guid id);
        VipLevel GetVipLevel(Guid id);
        TransferSettings GetTransferSettings(Guid id);
        Player GetPlayer(Guid playerId);
        Player GetPlayerWithBank(Guid playerId);
        Player GetPlayerForNewBankAccount(PlayerId playerId);
        PaymentLevel GetPaymentLevel(Guid id);
        List<OfflineWithdrawalHistory> WithdrawalHistories(Guid wdId);
        PaymentSettings GetOnlinePaymentSettings(Guid brandId, PaymentType type, string vipLevel,
            string paymentGatewayName, string currencyCode);
        PaymentSettings GetOfflinePaymentSettings(Guid brandId, PaymentType type, string vipLevel, 
            string currencyCode);
        Currency GetCurrency(string currencyCode);
        CurrencyExchange GetCurrencyExchange(Guid brandId, string currencyCode);
        DateTimeOffset GetBrandDateTimeOffset(Guid brandId);
        decimal GetWithdrawalLockBalance(Guid playerId);
        IEnumerable<BankAccount> GetBankAccountsForOfflineDeposit(Guid playerId);
        BankAccount GetBankAccountForOfflineDeposit(Guid offlineDepositId);

        #region Queryable Methods
        IQueryable<BankAccount> GetBankAccounts();
        IQueryable<Bank> GetBanks();
        IQueryable<PaymentSettings> GetPaymentSettings();
        IQueryable<PaymentLevel> GetPaymentLevelsAsQueryable();
        IQueryable<AFT.RegoV2.Core.Payment.Interface.Data.Brand> GetBrands();
        IQueryable<Currency> GetCurrencies();
        IQueryable<OfflineWithdraw> GetOfflineWithdraws();
        IQueryable<CurrencyExchange> GetCurrencyExchanges();
        IQueryable<CurrencyExchange> GetCurrencyExchangesbyBrand(Guid brandId);
        IQueryable<Licensee> GetLicensees();
        IQueryable<BrandCurrency> GetBrandCurrencies(Guid brandId);
        IQueryable<DepositDto> GetDeposits();
        IQueryable<DepositDto> GetDepositsAsQueryable();
        IQueryable<PaymentGatewaySettings> GetPaymentGatewaySettings();
        IQueryable<PlayerPaymentLevel> GetPlayerPaymentLevels();
        IQueryable<OfflineDeposit> GetOfflineDeposits();
        IQueryable<PlayerBankAccount> GetPlayerBankAccounts();
        IQueryable<TransferSettings> GetTransferSettings();
        IQueryable<VipLevel> VipLevels();
        IQueryable<PlayerPaymentAmount> GetPlayerDepositAmount(GetPlayerPaymentAmountRequest request);
        IQueryable<PlayerPaymentAmount> GetPlayerWithdrawalAmount(GetPlayerPaymentAmountRequest request);
        #endregion

        ValidationResult ValidatePlayerBankAccount(EditPlayerBankAccountData data);
        ValidationResult ValidateOfflineWithdrawalRequest(OfflineWithdrawRequest data);
        ValidationResult ValidateOfflineDepositRequest(OfflineDepositConfirm data);
    }
}
