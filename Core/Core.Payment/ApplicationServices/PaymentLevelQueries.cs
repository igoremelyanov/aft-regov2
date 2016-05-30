using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AutoMapper.QueryableExtensions;
using PaymentLevel= AFT.RegoV2.Core.Payment.Interface.Data.PaymentLevel;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{    
    public class PaymentLevelQueries : IApplicationService, IPaymentLevelQueries
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IBrandRepository _brandRepository;
        private readonly PlayerQueries _playerQueries;
        private readonly BrandQueries _brandQueries;

        static PaymentLevelQueries()
        {
            MapperConfig.CreateMap();
        }

        public PaymentLevelQueries(
            IPaymentRepository repository, 
            IPaymentQueries paymentQueries, 
            IBrandRepository brandRepository, 
            PlayerQueries playerQueries,
            BrandQueries brandQueries)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _brandRepository = brandRepository;
            _playerQueries = playerQueries;
            _brandQueries = brandQueries;
        }

        public IEnumerable<string> GetBrandCurrencies(Guid brandId)
        {
            var brand = _brandRepository.Brands
                .Include(b => b.BrandCurrencies)
                .Single(b => b.Id == brandId);

            return brand.BrandCurrencies.Select(c => c.CurrencyCode);
        }

        public PaymentLevelTransferObj GetPaymentLevelById(Guid id)
        {
            var level = _paymentQueries.GetPaymentLevel(id);
            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(level.BrandId, level.CurrencyCode);

            var obj = new PaymentLevelTransferObj
            {
                Brand = new
                {
                    Id = level.Brand.Id,
                    Name = level.Brand.Name,
                    Licensee = new
                    {
                        Id = level.Brand.LicenseeId,
                        Name = level.Brand.LicenseeName
                    }
                },
                Currency = level.CurrencyCode,
                Code = level.Code,
                Name = level.Name,
                EnableOfflineDeposit = level.EnableOfflineDeposit,
                EnableOnlineDeposit = level.EnableOnlineDeposit,
                IsDefault = defaultPaymentLevelId == level.Id,
                BankAccounts = level.BankAccounts.Select(x => x.Id),
                PaymentGatewaySettings = level.PaymentGatewaySettings.Select((x => x.Id)),
                MaxBankFee = level.MaxBankFee,
                BankFeeRatio = level.BankFeeRatio
            };

            return obj;
        }

        public IQueryable<PaymentLevel> GetReplacementPaymentLevels(Guid id)
        {
            var paymentLevel = _repository.PaymentLevels.Single(x => x.Id == id);

            var replacementPaymentLevels = _repository.PaymentLevels
                .Where(x => 
                    x.Id != id &&
                    x.BrandId == paymentLevel.BrandId &&
                    x.CurrencyCode == paymentLevel.CurrencyCode &&
                    x.Status == PaymentLevelStatus.Active)
                    .OrderBy(x => x.Name)
                    .Project().To<PaymentLevel>();
            return replacementPaymentLevels;
        }

        public DeactivatePaymentLevelStatus GetDeactivatePaymentLevelStatus(Guid id)
        {
            var paymentLevel = _repository.PaymentLevels.Single(x => x.Id == id);

            if (paymentLevel.Status == PaymentLevelStatus.Inactive) 
                return DeactivatePaymentLevelStatus.CannotDeactivateStatusInactive;

            var isInUse = _playerQueries.GetPlayersByPaymentLevel(id).Any();
            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(paymentLevel.BrandId, paymentLevel.CurrencyCode);
            var replacementRequired = paymentLevel.Id == defaultPaymentLevelId || isInUse;

            if (!replacementRequired)
                return DeactivatePaymentLevelStatus.CanDeactivate;

            var isReplacementAvailable = _repository.PaymentLevels.Any(x =>
                x.Id != id &&
                x.BrandId == paymentLevel.BrandId &&
                x.CurrencyCode == paymentLevel.CurrencyCode &&
                x.Status == PaymentLevelStatus.Active);

            if (!isReplacementAvailable)
                return DeactivatePaymentLevelStatus.CannotDeactivateNoReplacement;

            return paymentLevel.Id == defaultPaymentLevelId
                ? DeactivatePaymentLevelStatus.CanDeactivateIsDefault
                : DeactivatePaymentLevelStatus.CanDeactivateIsAssigned;
        }

        public IQueryable<PaymentLevel> GetPaymentLevelsByBrandAndCurrency(Guid brandId, string currencyCode)
        {
            var paymentLevels = _repository.PaymentLevels
                .Where(
                    x => x.BrandId == brandId && x.CurrencyCode == currencyCode && x.Status == PaymentLevelStatus.Active)
                .Include(x => x.Brand)
                .Project().To<PaymentLevel>();
            return paymentLevels;
        }
    }     
}
