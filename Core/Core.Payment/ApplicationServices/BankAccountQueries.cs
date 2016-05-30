using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using Microsoft.Practices.ObjectBuilder2;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bank = AFT.RegoV2.Core.Payment.Interface.Data.Bank;
using BankAccount = AFT.RegoV2.Core.Payment.Interface.Data.BankAccount;
using BankAccountType = AFT.RegoV2.Core.Payment.Interface.Data.BankAccountType;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class BankAccountQueries : IBankAccountQueries
    {
        private readonly IPaymentRepository _repository;
        private readonly BrandQueries _brandQueries;
        private readonly IAdminQueries _adminQueries;

        static BankAccountQueries()
        {
            MapperConfig.CreateMap();
        }

        public BankAccountQueries(IPaymentRepository repository, BrandQueries brandQueries, IAdminQueries adminQueries)
        {
            _repository = repository;
            _brandQueries = brandQueries;
            _adminQueries = adminQueries;
        }

        public BankAccount GetBankAccount(Guid id)
        {
            var query = _repository.BankAccounts
                .Include(x => x.Bank)
                .Include(x => x.PaymentLevels)
                .SingleOrDefault(x => x.Id == id);

            return Mapper.Map<BankAccount>(query);
        }

        public BankAccount GetBankAccountFull(Guid id)
        {
            var query = _repository.BankAccounts
                .Include(x => x.Bank)
                .Include(x => x.Bank.Brand)
                .Include(x => x.PaymentLevels)
                .Include(x => x.AccountType)
                .SingleOrDefault(x => x.Id == id);

            return Mapper.Map<BankAccount>(query);
        }

        public IQueryable<BankAccount> GetBankAccounts()
        {
            var query = _repository.BankAccounts
                .Include(x => x.Bank)
                .Include(x => x.AccountType)
                .Project().To<BankAccount>();

            return query;
        }

        public IQueryable<BankAccount> GetFilteredBankAccounts(Guid userId, string currencyCode = null)
        {
            var filteredBankAccounts = GetFilteredBankAccountsForUser(userId);

            if (!string.IsNullOrEmpty(currencyCode))
                filteredBankAccounts = FilterBankAccountsByCurrency(filteredBankAccounts, currencyCode);

            return filteredBankAccounts;
        }

        public IQueryable<Bank> GetBanks()
        {
            return _repository.Banks.Include(b => b.Brand)
                .Project().To<Bank>();
        }

        public IQueryable<BankAccount> GetFilteredBankAccountsForUser(Guid userId)
        {
            var filteredBrandIds = _brandQueries.GetFilteredBrands(_brandQueries.GetBrands(), userId)
                .ToArray()
                .Select(x => x.Id);

            var filteredBankAccounts = _repository.BankAccounts
                .Include(x => x.Bank.Brand)
                .Include(x => x.AccountType)
                .Where(x => filteredBrandIds.Contains(x.Bank.BrandId))
                .Project().To<BankAccount>(null, dest => dest.PaymentLevels);

            return filteredBankAccounts;
        }

        public IQueryable<BankAccount> FilterBankAccountsByLicensee(IQueryable<BankAccount> bankAccounts, Guid licenseeId)
        {
            var licenseeBrandIds = _brandQueries.GetLicensee(licenseeId)
                .Brands
                .ToArray()
                .Select(x => x.Id);

            var query= bankAccounts
                       .Where(x => licenseeBrandIds.Contains(x.Bank.BrandId))
                       .Project().To<BankAccount>();

            return query;
        }

        public IQueryable<BankAccount> FilterBankAccountsByBrand(IQueryable<BankAccount> bankAccounts, Guid brandId)
        {
            var query = bankAccounts.Where(x => x.Bank.BrandId == brandId)
                        .Project().To<BankAccount>();
            return query;
        }

        public IQueryable<BankAccount> FilterBankAccountsByCurrency(IQueryable<BankAccount> bankAccounts, string currencyCode)
        {
            var query = bankAccounts.Where(x => x.CurrencyCode == currencyCode)
                        .Project().To<BankAccount>();
            return query;
        }

        public object GetBankAccountById(Guid id)
        {
            var bankAccount = GetBankAccount(id);

            return new
            {
                BankAccount = new
                {
                    bankAccount.AccountId,
                    bankAccount.AccountName,
                    bankAccount.AccountNumber,
                    bankAccount.AccountType,
                    bankAccount.Branch,
                    bankAccount.Province,
                    bankAccount.Remarks,
                    bankAccount.CurrencyCode,
                    isAssignedToAnyPaymentLevel = bankAccount.PaymentLevels.Any()
                },
                Bank = new
                {
                    bankAccount.Bank.Id,
                    bankAccount.Bank.BrandId,
                    LicenseeId = _brandQueries.GetBrandOrNull(bankAccount.Bank.BrandId).Licensee.Id
                }
            };
        }

        public IEnumerable<string> GetCurrencies(Guid brandId)
        {
            var brand = _brandQueries.GetBrands()
                .Single(b => b.Id == brandId);

            var currencies = brand.BrandCurrencies
                .Select(c => c.CurrencyCode);

            return currencies;
        }

        public IQueryable<Bank> GetBanks(Guid brandId)
        {
            return GetBanks()
                .Where(b => b.Brand.Id == brandId);
        }
        
        public IOrderedEnumerable<string> GetCurrencyData(Guid userId)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

            var bankAccounts = GetFilteredBankAccountsForUser(userId)
                .ToList()
                .Where(x => brandFilterSelections.Contains(x.Bank.BrandId));

            var currencySet = new HashSet<string>();

            bankAccounts.ForEach(x => currencySet.Add(x.CurrencyCode));

            var currencyData = currencySet.OrderBy(x => x);

            return currencyData;
        }

        public IQueryable<BankAccountType> GetBankAccountTypes()
        {
            return _repository.BankAccountTypes
                .Project().To<BankAccountType>();
        }
    }
}
