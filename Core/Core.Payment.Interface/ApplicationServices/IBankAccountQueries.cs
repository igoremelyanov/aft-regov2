using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IBankAccountQueries
    {
        BankAccount GetBankAccount(Guid id);
        BankAccount GetBankAccountFull(Guid id);
        IQueryable<BankAccount> GetBankAccounts();
        IQueryable<BankAccount> GetFilteredBankAccounts(Guid userId, string currencyCode = null);

        IQueryable<Bank> GetBanks();
        IQueryable<BankAccount> GetFilteredBankAccountsForUser(Guid userId);
        IQueryable<BankAccount> FilterBankAccountsByLicensee(IQueryable<BankAccount> bankAccounts, Guid licenseeId);
        IQueryable<BankAccount> FilterBankAccountsByBrand(IQueryable<BankAccount> bankAccounts, Guid brandId);
        IQueryable<BankAccount> FilterBankAccountsByCurrency(IQueryable<BankAccount> bankAccounts, string currencyCode);
        object GetBankAccountById(Guid id);
        IEnumerable<string> GetCurrencies(Guid brandId);
        IQueryable<Bank> GetBanks(Guid brandId);
        IOrderedEnumerable<string> GetCurrencyData(Guid userId);
        IQueryable<BankAccountType> GetBankAccountTypes();
    }
}
