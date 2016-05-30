using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using FluentValidation.Results;
using BankAccount = AFT.RegoV2.Core.Payment.Data.BankAccount;
using PaymentLevel = AFT.RegoV2.Core.Payment.Data.PaymentLevel;
using PaymentGatewaySettings = AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentLevelCommands : IApplicationService, IPaymentLevelCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IBrandRepository _brandRepository;
        private readonly PlayerCommands _playerCommands;
        private readonly IPaymentLevelQueries _paymentLevelQueries;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;
        private readonly BrandCommands _brandCommands;

        public PaymentLevelCommands(
            IPaymentRepository repository,
            IPaymentQueries paymentQueries,
            IBrandRepository brandRepository,
            PlayerCommands playerCommands,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus,
            IPaymentLevelQueries paymentLevelQueries, BrandCommands brandCommands)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _brandRepository = brandRepository;
            _playerCommands = playerCommands;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
            _paymentLevelQueries = paymentLevelQueries;
            _brandCommands = brandCommands;
        }

        [Permission(Permissions.Create, Module = Modules.PaymentLevelManager)]
        public PaymentLevelSaveResult Save(EditPaymentLevel model)
        {
            Brand.Interface.Data.Brand brand = null;

            if (model.Brand.HasValue)
                brand = _brandRepository.Brands.Include(b => b.BrandCurrencies).SingleOrDefault(b => b.Id == model.Brand);

            ValidateCreatePaymentLevelModel(model, brand);

            var currency = brand.BrandCurrencies.Single(c => c.CurrencyCode == model.Currency);
            var currencyCode = currency.CurrencyCode;

            var bankAccounts = GetBankAccounts(model);

            FillBankReceipts(model, GetBankAccounts(model));

            var paymentGatewaySettings = GetPaymentGatewaySettings(model);

            var now = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);

            var paymentLevel = new PaymentLevel
            {
                Id = model.Id ?? Guid.NewGuid(),
                BrandId = brand.Id,
                CurrencyCode = currencyCode,
                Status = PaymentLevelStatus.Active,
                CreatedBy = _actorInfoProvider.Actor.UserName,
                DateCreated = now,
                ActivatedBy = _actorInfoProvider.Actor.UserName,
                DateActivated = now,
                Code = model.Code,
                Name = model.Name,
                EnableOfflineDeposit = model.EnableOfflineDeposit,
                EnableOnlineDeposit = model.EnableOnlineDeposit,
                BankFeeRatio = model.BankFeeRatio,
                MaxBankFee = model.MaxBankFee
            };

            _repository.PaymentLevels.Add(paymentLevel);

            if (model.IsDefault)
                _brandCommands.MakePaymentLevelDefault(paymentLevel.Id, paymentLevel.BrandId, currencyCode);

            paymentLevel.BankAccounts = new List<BankAccount>();

            if (bankAccounts != null)
            {
                foreach (var bankAccount in bankAccounts.Where(x => x.Status == BankAccountStatus.Active))
                    paymentLevel.BankAccounts.Add(bankAccount);
            }

            paymentLevel.PaymentGatewaySettings = new List<PaymentGatewaySettings>();
            if (paymentGatewaySettings != null)
            {
                foreach (var settings in paymentGatewaySettings)
                    paymentLevel.PaymentGatewaySettings.Add(settings);
            }

            _repository.SaveChanges();

            _eventBus.Publish(new PaymentLevelAdded(
                paymentLevel.Id,
                paymentLevel.Code,
                paymentLevel.Name,
                paymentLevel.BrandId,
                paymentLevel.CurrencyCode,
                paymentLevel.Status,
                paymentLevel.CreatedBy,
                paymentLevel.DateCreated)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
            });

            return new PaymentLevelSaveResult
            {
                Message = "app:payment.levelCreated",
                PaymentLevelId = paymentLevel.Id
            };
        }

        private void ValidateCreatePaymentLevelModel(EditPaymentLevel model, Brand.Interface.Data.Brand brand)
        {
            if (brand == null)
                throw new RegoException("app:common.invalidBrand");

            var currency = brand.BrandCurrencies.SingleOrDefault(c => c.CurrencyCode == model.Currency);
            if (currency == null)
                throw new RegoException("app:payment.invalidCurrency");

            var paymentLevels = _paymentQueries.GetPaymentLevels(brand.Id);
            if (paymentLevels.Any(pl => pl.Name == model.Name && pl.BrandId == brand.Id && pl.Id != model.Id))
                throw new RegoException("app:payment.levelNameUnique");

            if (paymentLevels.Any(pl => pl.Code == model.Code && pl.BrandId == brand.Id && pl.Id != model.Id))
                throw new RegoException("app:payment.levelCodeUnique");

            if (model.IsDefault &&
                paymentLevels.Any(
                    pl => pl.Id != model.Id && pl.BrandId == model.Brand && pl.CurrencyCode == model.Currency && pl.IsDefault))
                throw new RegoException("Default payment level for the brand and currency combination already exists.");
        }

        [Permission(Permissions.Update, Module = Modules.PaymentLevelManager)]
        public PaymentLevelSaveResult Edit(EditPaymentLevel model)
        {
            var id = model.Id;

            var paymentLevel = _repository.PaymentLevels
                .Include(l => l.Brand)
                .Include(l => l.BankAccounts)
                .SingleOrDefault(l => l.Id == id.Value);

            if (paymentLevel == null)
                throw new RegoException("app:common.invalidId");

            var currencyCode = paymentLevel.CurrencyCode;
            Guid? brandId = paymentLevel.BrandId;

            var paymentLevels = _paymentQueries.GetPaymentLevels(brandId.Value);
            if (paymentLevels.Any(pl => pl.Name == model.Name && pl.BrandId == brandId && pl.Id != model.Id))
                throw new RegoException("app:payment.levelNameUnique");

            if (paymentLevels.Any(pl => pl.Code == model.Code && pl.BrandId == brandId && pl.Id != model.Id))
                throw new RegoException("app:payment.levelCodeUnique");

            if (model.IsDefault && paymentLevels.Any(pl => pl.Id != model.Id && pl.BrandId == model.Brand && pl.CurrencyCode == model.Currency && pl.IsDefault))
                throw new RegoException("Default payment level for the brand and currency combination already exists.");

            var bankAccounts = GetBankAccounts(model);

            FillBankReceipts(model, GetBankAccounts(model));

            var paymentGatewaySettings = GetPaymentGatewaySettings(model);

            var now = DateTimeOffset.Now.ToBrandOffset(paymentLevel.Brand.TimezoneId);

            paymentLevel.UpdatedBy = _actorInfoProvider.Actor.UserName;
            paymentLevel.DateUpdated = now;

            paymentLevel.Code = model.Code;
            paymentLevel.Name = model.Name;
            paymentLevel.EnableOfflineDeposit = model.EnableOfflineDeposit;
            paymentLevel.EnableOnlineDeposit = model.EnableOnlineDeposit;
            paymentLevel.BankFeeRatio = model.BankFeeRatio;
            paymentLevel.MaxBankFee = model.MaxBankFee;

            paymentLevel.CurrencyCode = currencyCode;
            paymentLevel.Id = (Guid)id;
            paymentLevel.BrandId = (Guid)brandId;

            if (model.IsDefault)
                _brandCommands.MakePaymentLevelDefault(paymentLevel.Id, paymentLevel.BrandId, currencyCode);

            paymentLevel.BankAccounts = new List<BankAccount>();
            if (bankAccounts != null)
            {
                foreach (var bankAccount in bankAccounts.Where(x => x.Status == BankAccountStatus.Active))
                    paymentLevel.BankAccounts.Add(bankAccount);
            }
            paymentLevel.PaymentGatewaySettings.Clear();
            if (paymentGatewaySettings != null)
            {
                foreach (var settings in paymentGatewaySettings)//filter status?
                    paymentLevel.PaymentGatewaySettings.Add(settings);
            }

            _repository.SaveChanges();
            _eventBus.Publish(new PaymentLevelEdited(
                paymentLevel.Id,
                paymentLevel.Code,
                paymentLevel.Name,
                paymentLevel.BrandId,
                paymentLevel.CurrencyCode,
                paymentLevel.Status,
                paymentLevel.CreatedBy,
                paymentLevel.DateCreated
                )
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(paymentLevel.Brand.TimezoneId),
            });

            return new PaymentLevelSaveResult
            {
                Message = "app:payment.levelUpdated",
                PaymentLevelId = paymentLevel.Id
            };
        }

        private void FillBankReceipts(EditPaymentLevel model, IQueryable<BankAccount> accounts)
        {
            foreach (var account in accounts)
            {
                account.InternetSameBank = model.InternetSameBankSelection.Contains(account.Id);
                account.AtmSameBank = model.AtmSameBankSelection.Contains(account.Id);
                account.CounterDepositSameBank = model.CounterDepositSameBankSelection.Contains(account.Id);
                account.InternetDifferentBank = model.InternetDifferentBankSelection.Contains(account.Id);
                account.AtmDifferentBank = model.AtmDifferentBankSelection.Contains(account.Id);
                account.CounterDepositDifferentBank = model.CounterDepositDifferentBankSelection.Contains(account.Id);
            }
        }

        private IQueryable<BankAccount> GetBankAccounts(EditPaymentLevel model)
        {
            var bankAccounts = Enumerable.Empty<BankAccount>().AsQueryable();

            if (model.BankAccounts != null)
            {
                var bankAccountIds = model.BankAccounts.Select(ba => new Guid(ba));
                bankAccounts = _repository.BankAccounts
                    .Include(o => o.Bank.Accounts.Select(t => t.AccountType))
                    .Where(ba => bankAccountIds.Contains(ba.Id));

                if (bankAccounts.Any(ba => ba.CurrencyCode != model.Currency))
                {
                    throw new RegoException("app:payment.levelAccountCurrencyMismatch");
                }
            }

            return bankAccounts;
        }

        private IEnumerable<PaymentGatewaySettings> GetPaymentGatewaySettings(EditPaymentLevel model)
        {
            List<PaymentGatewaySettings> paymentGatewaySettings = null;
            if (model.PaymentGatewaySettings != null)
            {
                var paymentGatewaySettingsIds = model.PaymentGatewaySettings.Select(ba => new Guid(ba));
                paymentGatewaySettings = _repository.PaymentGatewaySettings
                .Include(x => x.Brand)
                .Where(ba => paymentGatewaySettingsIds.Contains(ba.Id)).ToList();
            }
            return paymentGatewaySettings;
        }

        public ValidationResult ValidatePaymentLevelCanBeActivated(ActivatePaymentLevelCommand command)
        {
            var validator = new ActivatePaymentLevelValidator(_repository);
            var validationResult = validator.Validate(command);
            return validationResult;
        }

        [Permission(Permissions.Activate, Module = Modules.PaymentLevelManager)]
        public void Activate(ActivatePaymentLevelCommand command)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var paymentLevel = _repository.PaymentLevels
                    .Include(x => x.Brand)
                    .Single(x => x.Id == command.Id);

                paymentLevel.Status = PaymentLevelStatus.Active;
                paymentLevel.ActivatedBy = _actorInfoProvider.Actor.UserName;

                paymentLevel.DateActivated = DateTimeOffset.Now.ToBrandOffset(paymentLevel.Brand.TimezoneId);

                _repository.SaveChanges();

                _eventBus.Publish(new PaymentLevelActivated
                {
                    Id = paymentLevel.Id,
                    Code = paymentLevel.Code,
                    Name = paymentLevel.Name,
                    ActivatedBy = paymentLevel.ActivatedBy,
                    ActivatedDate = paymentLevel.DateActivated.Value,
                    Remarks = command.Remarks,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(paymentLevel.Brand.TimezoneId),
            });

                scope.Complete();
            }
        }

        public ValidationResult ValidatePaymentLevelCanBeDeactivated(DeactivatePaymentLevelCommand command)
        {
            var validator = new DeactivatePaymentLevelValidator(_repository, _paymentLevelQueries);
            var validationResult = validator.Validate(command);
            return validationResult;
        }

        [Permission(Permissions.Deactivate, Module = Modules.PaymentLevelManager)]
        public void Deactivate(DeactivatePaymentLevelCommand command)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var oldPaymentLevel = _repository.PaymentLevels
                    .Include(x => x.Brand)
                    .Single(x => x.Id == command.Id);

                oldPaymentLevel.Status = PaymentLevelStatus.Inactive;
                oldPaymentLevel.DeactivatedBy = _actorInfoProvider.Actor.UserName;

                oldPaymentLevel.DateDeactivated = DateTimeOffset.Now.ToBrandOffset(oldPaymentLevel.Brand.TimezoneId);

                if (command.NewPaymentLevelId.HasValue)
                {
                    _playerCommands.UpdatePlayersPaymentLevel(command.Id, command.NewPaymentLevelId.Value);

                    var newPaymentLevel = _repository.PaymentLevels.Single(x => x.Id == command.NewPaymentLevelId);

                    _brandCommands.MakePaymentLevelDefault(newPaymentLevel.Id, newPaymentLevel.BrandId, newPaymentLevel.CurrencyCode);

                    _repository.PlayerPaymentLevels
                        .Include(x => x.PaymentLevel)
                        .Where(x => x.PaymentLevel.Id == command.Id)
                        .ForEach(x => x.PaymentLevel = newPaymentLevel);
                }

                _repository.SaveChanges();

                _eventBus.Publish(new PaymentLevelDeactivated
                {
                    Id = oldPaymentLevel.Id,
                    Code = oldPaymentLevel.Code,
                    Name = oldPaymentLevel.Name,
                    DeactivatedBy = oldPaymentLevel.DeactivatedBy,
                    DeactivatedDate = oldPaymentLevel.DateDeactivated.Value,
                    Remarks = command.Remarks,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(oldPaymentLevel.Brand.TimezoneId),
                });

                scope.Complete();
            }
        }
    }
}
