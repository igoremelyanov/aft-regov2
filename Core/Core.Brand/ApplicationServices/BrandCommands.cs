using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using FluentValidation;
using FluentValidation.Results;
using BrandLanguagesAssigned = AFT.RegoV2.Core.Common.Events.Brand.BrandLanguagesAssigned;
using Country = AFT.RegoV2.Core.Brand.Interface.Data.Country;
using VipLevel = AFT.RegoV2.Core.Brand.Interface.Data.VipLevel;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public interface IBrandCommands : IApplicationService
    {
        Guid AddBrand(AddBrandRequest addBrandRequest);
        void EditBrand(EditBrandRequest editBrandRequest);
        ValidationResult ValidateThatBrandCanBeActivated(ActivateBrandRequest activateBrandRequest);
        ValidationResult ValidateThatBrandCanBeDeactivated(DeactivateBrandRequest activateBrandRequest);
        ValidationResult ValidateThatBrandCountryCanBeAssigned(AssignBrandCountryRequest request);
        ValidationResult ValidateThatBrandCurrencyCanBeAssigned(AssignBrandCurrencyRequest request);
        void ActivateBrand(ActivateBrandRequest activateBrandRequest);
        void DeactivateBrand(DeactivateBrandRequest deactivateBrandRequest);
        void AssignBrandCulture(AssignBrandCultureRequest assignBrandCultureData);
        void AssignBrandCountry(AssignBrandCountryRequest assignBrandCountryRequest);
        void AssignBrandCurrency(AssignBrandCurrencyRequest assignBrandCurrencyRequest);
        void AssignBrandProducts(AssignBrandProductsData assignBrandProductsData);
        void ActivateCulture(string code, string remarks);
        void DeactivateCulture(string code, string remarks);
        ValidationResult ValidateThatVipLevelCanBeAdded(VipLevelViewModel model);
        Guid AddVipLevel(VipLevelViewModel model);
        void SetDefaultVipLevel(Interface.Data.Brand brand, Guid vipLevelId);
        void EditVipLevel(VipLevelViewModel model);
        void CreateWalletStructureForBrand(WalletTemplateViewModel viewModel);
        void UpdateWalletStructureForBrand(WalletTemplateViewModel viewModel);
        void ActivateVipLevel(Guid vipLevelId, string remark);
        void DeactivateVipLevel(Guid deactivateVipLevelId, string remark, Guid? newDefaultVipLevelId);
        void CreateCountry(string code, string name);
        void UpdateCountry(string code, string name);
        void DeleteCountry(string code);
    }

    public class BrandCommands : MarshalByRefObject, IBrandCommands
    {
        private readonly IBrandRepository _repository;
        private readonly BrandQueries _queries;
        private readonly IGameQueries _gameQueries;
        private readonly IBasePaymentQueries _paymentQueries;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IAdminCommands _adminCommands;

        public BrandCommands(
            IBrandRepository repository,
            BrandQueries queries,
            IGameQueries gameQueries,
            IBasePaymentQueries paymentQueries,
            IEventBus eventBus,
            IActorInfoProvider actorInfoProvider,
            IAdminCommands adminCommands)
        {
            _repository = repository;
            _queries = queries;
            _gameQueries = gameQueries;
            _paymentQueries = paymentQueries;
            _eventBus = eventBus;
            _actorInfoProvider = actorInfoProvider;
            _adminCommands = adminCommands;
        }

        public Guid AddBrand(AddBrandRequest addBrandRequest)
        {
            var validationResult = ValidateThatBrandCanBeAdded(addBrandRequest);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = new Interface.Data.Brand
            {
                Id = addBrandRequest.Id ?? Guid.NewGuid(),
                LicenseeId = addBrandRequest.Licensee,
                Licensee = _repository.Licensees.Single(x => x.Id == addBrandRequest.Licensee),
                Code = addBrandRequest.Code,
                Name = addBrandRequest.Name,
                Email = addBrandRequest.Email,
                SmsNumber = addBrandRequest.SmsNumber,
                WebsiteUrl = Uri.EscapeUriString(addBrandRequest.WebsiteUrl),
                Type = addBrandRequest.Type,
                TimezoneId = addBrandRequest.TimeZoneId,
                EnablePlayerPrefix = addBrandRequest.EnablePlayerPrefix,
                PlayerPrefix = addBrandRequest.PlayerPrefix,
                PlayerActivationMethod = addBrandRequest.PlayerActivationMethod,
                Status = BrandStatus.Inactive,
                CreatedBy = _actorInfoProvider.Actor.UserName,
                DateCreated = DateTimeOffset.Now.ToBrandOffset(addBrandRequest.TimeZoneId)
            };

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Brands.Add(brand);
                _repository.SaveChanges();

                _adminCommands.AddBrandToAdmin(_actorInfoProvider.Actor.Id, brand.Id);
                _eventBus.Publish(new BrandRegistered
                {
                    Id = brand.Id,
                    Code = brand.Code,
                    Name = brand.Name,
                    Email = brand.Email,
                    SmsNumber = brand.SmsNumber,
                    WebsiteUrl = brand.WebsiteUrl,
                    LicenseeId = brand.Licensee.Id,
                    LicenseeName = brand.Licensee.Name,
                    TimeZoneId = brand.TimezoneId,
                    BrandType = brand.Type,
                    Status = brand.Status,
                    PlayerPrefix = brand.PlayerPrefix,
                    InternalAccountsNumber = brand.InternalAccountsNumber,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });
                scope.Complete();
            }

            return brand.Id;
        }

        public void EditBrand(EditBrandRequest editBrandData)
        {
            var validationResult = ValidateThatBrandCanBeEdited(editBrandData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = _repository.Brands.Single(x => x.Id == editBrandData.Brand);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.Licensee = _repository.Licensees.Single(x => x.Id == editBrandData.Licensee);
                brand.Type = editBrandData.Type;
                brand.Name = editBrandData.Name;
                brand.Code = editBrandData.Code;
                brand.Email = editBrandData.Email;
                brand.SmsNumber = editBrandData.SmsNumber;
                brand.WebsiteUrl = Uri.EscapeUriString(editBrandData.WebsiteUrl);
                brand.EnablePlayerPrefix = editBrandData.EnablePlayerPrefix;
                brand.PlayerPrefix = editBrandData.PlayerPrefix;
                brand.PlayerActivationMethod = editBrandData.PlayerActivationMethod;
                brand.InternalAccountsNumber = editBrandData.InternalAccounts;
                brand.TimezoneId = editBrandData.TimeZoneId;
                brand.Remarks = editBrandData.Remarks;
                brand.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                brand.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandUpdated
                {
                    Id = brand.Id,
                    Code = brand.Code,
                    Name = brand.Name,
                    Email = brand.Email,
                    SmsNumber = brand.SmsNumber,
                    WebsiteUrl = brand.WebsiteUrl,
                    LicenseeId = brand.Licensee.Id,
                    LicenseeName = brand.Licensee.Name,
                    TypeName = brand.Type.ToString(),
                    Remarks = brand.Remarks,
                    PlayerPrefix = brand.PlayerPrefix,
                    TimeZoneId = brand.TimezoneId,
                    InternalAccountCount = brand.InternalAccountsNumber,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });
                scope.Complete();
            }
        }

        public ValidationResult ValidateThatBrandCanBeAdded(AddBrandRequest request)
        {
            var validator = new AddBrandValidator(_repository);
            return validator.Validate(request);
        }

        public ValidationResult ValidateThatBrandCanBeEdited(EditBrandRequest request)
        {
            var validator = new EditBrandValidator(_repository);
            return validator.Validate(request);
        }

        public ValidationResult ValidateThatBrandCanBeActivated(ActivateBrandRequest request)
        {
            var brand = GetBrandForActivation(request.BrandId);
            var activateBrandData = new ActivateBrandValidationData
            {
                Brand = brand,
                BrandPaymentLevels = _paymentQueries.GetPaymentLevels(request.BrandId).ToArray(),
                BrandRiskLevels = _queries.GetRiskLevels(request.BrandId).ToArray(),
                Remarks = request.Remarks
            };
            var validator = new ActivateBrandValidator();
            return validator.Validate(activateBrandData);
        }

        public ValidationResult ValidateThatBrandCanBeDeactivated(DeactivateBrandRequest request)
        {
            var brand = GetBrandForDeactivation(request.BrandId);
            brand.Remarks = request.Remarks;
            var deactivateBrandData = new DeactivateBrandValidationData
            {
                Brand = brand
            };
            var validator = new DeactivateBrandValidator();
            return validator.Validate(deactivateBrandData);
        }

        public ValidationResult ValidateThatBrandCountryCanBeAssigned(AssignBrandCountryRequest request)
        {
            var validator = new AssignBrandCountryValidator(_repository);
            return validator.Validate(request);
        }

        public ValidationResult ValidateThatBrandCultureCanBeAssigned(AssignBrandCultureRequest request)
        {
            var validator = new AssignBrandCultureValidator(_repository);
            return validator.Validate(request);
        }

        public ValidationResult ValidateThatBrandCurrencyCanBeAssigned(AssignBrandCurrencyRequest request)
        {
            var validator = new AssignBrandCurrencyValidator(_repository);
            return validator.Validate(request);
        }

        public ValidationResult ValidateThatBrandProductsCanBeAssigned(AssignBrandProductsData data)
        {
            var validator = new AssignBrandProductValidator(_repository,
                _queries.GetAllowedProductsByBrand(data.BrandId));
            return validator.Validate(data);
        }

        private Interface.Data.Brand GetBrandForActivation(Guid brandId)
        {
            var brand = _repository.Brands
                .Include(b => b.BrandCountries.Select(x => x.Country))
                .Include(b => b.BrandCultures.Select(x => x.Culture))
                .Include(b => b.BrandCurrencies.Select(x => x.Currency))
                .Include(b => b.VipLevels)
                .Include(b => b.WalletTemplates)
                .Include(b => b.Licensee.Brands)
                .Include(b => b.Products)
                .Include(b => b.DefaultVipLevel)
                .Single(b => b.Id == brandId);

            return brand;
        }

        private Interface.Data.Brand GetBrandForDeactivation(Guid brandId)
        {
            var brand = _repository.Brands
                .Single(b => b.Id == brandId);

            return brand;
        }

        public void ActivateBrand(ActivateBrandRequest request)
        {
            var brand = GetBrandForActivation(request.BrandId);

            var validationResult = ValidateThatBrandCanBeActivated(request);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.Status = BrandStatus.Active;
                brand.UpdatedBy = brand.ActivatedBy = _actorInfoProvider.Actor.UserName;
                brand.DateUpdated = brand.DateActivated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                brand.Remarks = request.Remarks;

                _repository.SaveChanges();

                _eventBus.Publish(
                    new BrandActivated
                    {
                        Id = brand.Id,
                        EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                    });
                scope.Complete();
            }
        }

        public void DeactivateBrand(DeactivateBrandRequest request)
        {
            var brand = _repository.Brands.SingleOrDefault(x => x.Id == request.BrandId);

            if (brand != null)
                brand.Remarks = request.Remarks;

            var deactivateBrandData = new DeactivateBrandValidationData { Brand = brand };

            var validationResult = new DeactivateBrandValidator().Validate(deactivateBrandData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.Status = BrandStatus.Deactivated;
                brand.UpdatedBy = brand.DeactivatedBy = _actorInfoProvider.Actor.UserName;
                brand.DateUpdated = brand.DateDeactivated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);

                _repository.SaveChanges();

                _eventBus.Publish(new BrandDeactivated
                {
                    Id = request.BrandId,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });
                scope.Complete();
            }
        }

        public void AssignBrandCulture(AssignBrandCultureRequest assignBrandCultureData)
        {
            var validationResult = new AssignBrandCultureValidator(_repository).Validate(assignBrandCultureData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = _repository.Brands
                .Include(x => x.BrandCultures)
                .Single(x => x.Id == assignBrandCultureData.Brand);
            
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var oldCultures = brand.BrandCultures
                    .Where(x => !assignBrandCultureData.Cultures.Contains(x.CultureCode))
                    .ToArray();

                foreach (var oldCulture in oldCultures)
                {
                    brand.BrandCultures.Remove(oldCulture);
                }

                var newCultures = assignBrandCultureData.Cultures
                    .Where(x => brand.BrandCultures.All(y => y.CultureCode != x))
                    .ToArray();

                foreach (var culture in newCultures)
                {
                    var cultureToAdd = _repository.Cultures.Single(x => x.Code == culture);

                    brand.BrandCultures.Add(new BrandCulture
                    {
                        BrandId = brand.Id,
                        Brand = brand,
                        CultureCode = cultureToAdd.Code,
                        Culture = cultureToAdd,
                        DateAdded = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                        AddedBy = _actorInfoProvider.Actor.UserName
                    });
                }

                brand.DefaultCulture = assignBrandCultureData.DefaultCulture;
                brand.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                brand.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandLanguagesAssigned(
                    brand.Id,
                    brand.Name,
                    brand.BrandCultures.Select(x => x.Culture),
                    brand.DefaultCulture)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                scope.Complete();

            }
        }

        public void AssignBrandCountry(AssignBrandCountryRequest assignBrandCountryRequest)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AssignBrandCountryValidator(_repository).Validate(assignBrandCountryRequest);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var brand = _repository.Brands
                    .Include(x => x.BrandCountries.Select(y => y.Country))
                    .Single(x => x.Id == assignBrandCountryRequest.Brand);

                var oldCountries = brand.BrandCountries
                    .Where(x => !assignBrandCountryRequest.Countries.Contains(x.CountryCode))
                    .ToArray();

                foreach (var oldCountry in oldCountries)
                {
                    brand.BrandCountries.Remove(oldCountry);
                }

                var newCountries = assignBrandCountryRequest.Countries
                    .Where(x => brand.BrandCountries.All(y => y.CountryCode != x))
                    .ToArray();

                foreach (var country in newCountries)
                {
                    var countryToAdd = _repository.Countries.Single(x => x.Code == country);

                    brand.BrandCountries.Add(new BrandCountry
                    {
                        BrandId = brand.Id,
                        Brand = brand,
                        CountryCode = countryToAdd.Code,
                        Country = countryToAdd,
                        DateAdded = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                        AddedBy = _actorInfoProvider.Actor.UserName
                    });
                }

                brand.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                brand.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandCountriesAssigned(brand)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                scope.Complete();
            }
        }

        public void AssignBrandCurrency(AssignBrandCurrencyRequest assignBrandCurrencyRequest)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = ValidateThatBrandCurrencyCanBeAssigned(assignBrandCurrencyRequest);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var brand = _repository.Brands
                    .Include(x => x.BrandCurrencies)
                    .Single(x => x.Id == assignBrandCurrencyRequest.Brand);

                if (brand.BrandCurrencies.Count == 0)
                {
                    brand.CurrencySetCreated = DateTime.Now;
                    brand.CurrencySetCreatedBy = Thread.CurrentPrincipal.Identity.Name;
                }
                else
                {
                    var oldCurrencies = brand.BrandCurrencies
                        .Where(x => !assignBrandCurrencyRequest.Currencies.Contains(x.CurrencyCode))
                        .ToArray();

                    foreach (var oldCurrency in oldCurrencies)
                    {
                        brand.BrandCurrencies.Remove(oldCurrency);
                    }

                    brand.CurrencySetUpdated = DateTime.Now;
                    brand.CurrencySetUpdatedBy = Thread.CurrentPrincipal.Identity.Name;
                }

                brand.DefaultCurrency = assignBrandCurrencyRequest.DefaultCurrency;
                brand.BaseCurrency = assignBrandCurrencyRequest.BaseCurrency;

                var newCurrencies =
                    assignBrandCurrencyRequest.Currencies.Where(x => brand.BrandCurrencies.All(y => y.CurrencyCode != x));

                foreach (var currency in newCurrencies
                    .Select(newCurrency => _repository.Currencies.Single(c => c.Code == newCurrency)))
                {
                    brand.BrandCurrencies.Add(new BrandCurrency
                    {
                        BrandId = brand.Id,
                        Brand = brand,
                        CurrencyCode = currency.Code,
                        Currency = currency,
                        DateAdded = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                        AddedBy = _actorInfoProvider.Actor.UserName
                    });
                }

                brand.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                brand.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();
                _eventBus.Publish(new BrandCurrenciesAssigned
                {
                    BrandId = brand.Id,
                    Currencies = brand.BrandCurrencies.Select(bc => bc.CurrencyCode).ToArray(),
                    DefaultCurrency = brand.DefaultCurrency,
                    BaseCurrency = brand.BaseCurrency,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                scope.Complete();
            }
        }

        public void AssignBrandProducts(AssignBrandProductsData assignBrandProductsData)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validator = new AssignBrandProductValidator(_repository,
                    _queries.GetAllowedProductsByBrand(assignBrandProductsData.BrandId));

                var validationResult = validator.Validate(assignBrandProductsData);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var brand = _repository.Brands
                    .Include(x => x.Products)
                    .Single(x => x.Id == assignBrandProductsData.BrandId);

                brand.Products.Clear();

                foreach (var product in assignBrandProductsData.ProductsIds)
                {
                    brand.Products.Add(new BrandProduct
                    {
                        Brand = brand,
                        ProductId = product
                    });
                }

                brand.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                brand.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _eventBus.Publish(new BrandProductsAssigned
                {
                    BrandId = brand.Id,
                    ProductsIds = assignBrandProductsData.ProductsIds,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.LanguageManager)]
        public void ActivateCulture(string code, string remarks)
        {
            UpdateCultureStatus(code, CultureStatus.Active, remarks);
        }

        [Permission(Permissions.Deactivate, Module = Modules.LanguageManager)]
        public void DeactivateCulture(string code, string remarks)
        {
            UpdateCultureStatus(code, CultureStatus.Inactive, remarks);
        }

        private void UpdateCultureStatus(string code, CultureStatus status, string remarks)
        {
            var culture = _repository.Cultures.First(x => x.Code == code);

            if (culture.Status == status)
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var user = _actorInfoProvider.Actor.UserName;
                culture.Status = status;
                culture.UpdatedBy = user;
                culture.DateUpdated = DateTimeOffset.Now;

                if (status == CultureStatus.Active)
                {
                    culture.ActivatedBy = user;
                    culture.DateActivated = culture.DateUpdated;
                }
                else
                {
                    culture.DeactivatedBy = user;
                    culture.DateDeactivated = culture.DateUpdated;
                }

                _repository.SaveChanges();

                var languageStatusChanged = new LanguageStatusChanged
                {
                    Code = culture.Code,
                    Status = culture.Status,
                    Remarks = remarks
                };
                _eventBus.Publish(languageStatusChanged);

                scope.Complete();
            }
        }

        public ValidationResult ValidateThatVipLevelCanBeAdded(VipLevelViewModel model)
        {
            var validator = new AddVipLevelValidator(
                _repository,
                _gameQueries.GetGameDtos());
            return validator.Validate(model);
        }

        [Permission(Permissions.Create, Module = Modules.VipLevelManager)]
        public Guid AddVipLevel(VipLevelViewModel model)
        {
            var validationResult = ValidateThatVipLevelCanBeAdded(model);
            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brand = _repository.Brands.Include(x => x.VipLevels).Single(x => x.Id == model.Brand);

                var vipLevel = new VipLevel
                {
                    Id = model.Id ?? Guid.NewGuid(),
                    BrandId = brand.Id,
                    Code = model.Code,
                    Name = model.Name,
                    Rank = model.Rank,
                    Description = model.Description,
                    ColorCode = model.Color,
                    CreatedBy = _actorInfoProvider.Actor.UserName,
                    DateCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId)
                };

                var vipLevelLimits = model.Limits.Select(x => new VipLevelGameProviderBetLimit
                {
                    Id = Guid.NewGuid(),
                    VipLevel = vipLevel,
                    GameProviderId = x.GameProviderId.Value,
                    Currency = _repository.Currencies.Single(y => y.Code == x.CurrencyCode),
                    BetLimitId = x.BetLimitId.Value
                }).ToList();

                vipLevel.VipLevelGameProviderBetLimits = vipLevelLimits;

                _repository.VipLevels.Add(vipLevel);
                brand.VipLevels.Add(vipLevel);
                _repository.SaveChanges();

                _eventBus.Publish(new VipLevelRegistered
                {
                    Id = vipLevel.Id,
                    BrandId = vipLevel.BrandId,
                    Code = vipLevel.Code,
                    Name = vipLevel.Name,
                    Rank = vipLevel.Rank,
                    Description = vipLevel.Description,
                    ColorCode = vipLevel.ColorCode,
                    Status = vipLevel.Status,
                   
                    VipLevelLimits = vipLevel.VipLevelGameProviderBetLimits.Select(x => new VipLevelLimitData
                    {
                        Id = x.Id,
                        VipLevelId = vipLevel.Id,
                        CurrencyCode = x.Currency.Code,
                        GameProviderId = x.GameProviderId,
                        BetLimitId = x.BetLimitId

                    }).ToArray(),
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                if (model.IsDefault)
                    SetDefaultVipLevel(brand, vipLevel.Id);

                scope.Complete();

                return vipLevel.Id;
            }
        }

        public void SetDefaultVipLevel(Interface.Data.Brand brand, Guid vipLevelId)
        {
            var oldVipLevelId = brand.DefaultVipLevelId;
            brand.DefaultVipLevelId = vipLevelId;

            _eventBus.Publish(new BrandDefaultVipLevelChanged
            {
                BrandId = brand.Id,
                OldVipLevelId = oldVipLevelId,
                DefaultVipLevelId = vipLevelId,
                EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
            });
            _repository.SaveChanges();
        }

        public ValidationResult ValidateThatVipLevelCanBeEdited(VipLevelViewModel model)
        {
            var validator = new EditVipLevelValidator(
                _repository,
                _gameQueries.GetGameDtos());
            return validator.Validate(model);
        }

        [Permission(Permissions.Update, Module = Modules.VipLevelManager)]
        public void EditVipLevel(VipLevelViewModel model)
        {
            var validationResult = ValidateThatVipLevelCanBeEdited(model);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var existingVipLevel = _repository
                    .VipLevels
                    .Include(x => x.VipLevelGameProviderBetLimits)
                    .Single(x => x.Id == model.Id);

                //update viplevel
                var brand = _repository.Brands.Single(x => x.Id == model.Brand);
                existingVipLevel.Brand = brand;
                existingVipLevel.Code = model.Code;
                existingVipLevel.Name = model.Name;
                existingVipLevel.Rank = model.Rank;
                existingVipLevel.Description = model.Description;
                existingVipLevel.ColorCode = model.Color;
                existingVipLevel.UpdatedBy = _actorInfoProvider.Actor.UserName;
                existingVipLevel.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                existingVipLevel.UpdatedRemark = model.Remark;
                //remove removed limits
                var removedLimits = existingVipLevel
                    .VipLevelGameProviderBetLimits
                    .Where(x => model.Limits.All(lvm => lvm.Id != x.Id))
                    .ToArray();

                removedLimits.ForEach(x =>
                {
                    existingVipLevel.VipLevelGameProviderBetLimits.Remove(x);
                    _repository.VipLevelLimits.Remove(x);
                });

                //updating viplimits
                foreach (var limitViewModel in model.Limits)
                {
                    var limit = existingVipLevel.
                        VipLevelGameProviderBetLimits
                        .SingleOrDefault(x => x.Id == limitViewModel.Id);


                    if (limit == null)
                    {
                        limit = new VipLevelGameProviderBetLimit()
                        {
                            Id = Guid.NewGuid(),
                            VipLevel = existingVipLevel,
                            GameProviderId = limitViewModel.GameProviderId.Value,
                            Currency = _repository.Currencies.Single(y => y.Code == limitViewModel.CurrencyCode),
                            BetLimitId = limitViewModel.BetLimitId.Value
                        };
                        existingVipLevel.VipLevelGameProviderBetLimits.Add(limit);
                    }
                    else
                    {
                        limit.VipLevel = existingVipLevel;
                        limit.GameProviderId = limitViewModel.GameProviderId.Value;
                        limit.Currency = _repository.Currencies.Single(y => y.Code == limitViewModel.CurrencyCode);
                        limit.BetLimitId = limitViewModel.BetLimitId.Value;
                    }
                }

                //save and publish
                _repository.SaveChanges();
                _eventBus.Publish(new VipLevelUpdated
                {
                    Id = existingVipLevel.Id,
                    BrandId = existingVipLevel.Brand.Id,
                    Code = existingVipLevel.Code,
                    Name = existingVipLevel.Name,
                    Rank = existingVipLevel.Rank,
                    Description = existingVipLevel.Description,
                    ColorCode = existingVipLevel.ColorCode,
                    Remark = model.Remark,
                    
                    VipLevelLimits = existingVipLevel.VipLevelGameProviderBetLimits.Select(x => new VipLevelLimitData
                    {
                        Id = x.Id,
                        VipLevelId = existingVipLevel.Id,
                        CurrencyCode = x.Currency.Code,
                        GameProviderId = x.GameProviderId,
                        BetLimitId = x.BetLimitId
                    }).ToArray(),
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });

                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Module = Modules.WalletManager)]
        public void CreateWalletStructureForBrand(WalletTemplateViewModel viewModel)
        {
            TransformToOneWalletModel(viewModel);

            var validationResult = new AddWalletValidator()
                .Validate(viewModel);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = _repository.Brands.Single(x => x.Id == viewModel.BrandId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallets = new List<WalletTemplate>();
                wallets.Add(CreateWalletTemplate(
                    viewModel.BrandId,
                    viewModel.MainWallet.Name,
                    viewModel.MainWallet.ProductIds,
                    true));

                foreach (var walletViewModel in viewModel.ProductWallets)
                {
                    wallets.Add(CreateWalletTemplate(
                        viewModel.BrandId,
                        walletViewModel.Name,
                        walletViewModel.ProductIds,
                        false));
                }
                _repository.SaveChanges();

                _eventBus.Publish(new WalletTemplateCreated
                {
                    BrandId = viewModel.BrandId,
                    WalletTemplates = wallets.Select(x => new WalletTemplateDto
                    {
                        Id = x.Id,
                        IsMain = x.IsMain,
                        Name = x.Name,
                        ProductIds = x.WalletTemplateProducts.Select(wt => wt.ProductId).ToArray()
                    }).ToArray(),
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                });
                scope.Complete();
            }
        }

        private WalletTemplate CreateWalletTemplate(
            Guid brandId,
            string name,
            IEnumerable<Guid> productIds,
            bool isMain)
        {
            var brand = _repository.Brands.Single(x => x.Id == brandId);
            var walletTemplate = new WalletTemplate
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                CreatedBy = _actorInfoProvider.Actor.Id,
                Name = name,
                CurrencyCode = brand.DefaultCurrency,
                IsMain = isMain
            };

            foreach (var productId in productIds)
            {
                var walletProduct = new WalletTemplateProduct { Id = Guid.NewGuid(), ProductId = productId };
                walletTemplate.WalletTemplateProducts.Add(walletProduct);
            }

            brand.WalletTemplates.Add(walletTemplate);

            return walletTemplate;
        }

        [Permission(Permissions.Update, Module = Modules.WalletManager)]
        public void UpdateWalletStructureForBrand(WalletTemplateViewModel viewModel)
        {
            TransformToOneWalletModel(viewModel);

            var validationResult = new EditWalletValidator().Validate(viewModel);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = _repository.Brands.Single(x => x.Id == viewModel.BrandId);

            var existingWallets = _repository
                .WalletTemplates
                .Include(x => x.Brand)
                .Where(x => x.Brand.Id == viewModel.BrandId && !x.IsMain)
                .ToArray();
            var walletTemplatesToDelete =
                existingWallets.Where(x => viewModel.ProductWallets.All(y => y.Id != x.Id)).ToArray();
            walletTemplatesToDelete.ForEach(x => _repository.WalletTemplates.Remove(x));

            var remainedWallets = viewModel.ProductWallets.Where(x => x.Id != null && x.Id != Guid.Empty).ToArray();
            remainedWallets = remainedWallets.Concat(new[] { viewModel.MainWallet }).ToArray();
            remainedWallets.ForEach(rw => UpdateWalletTemplate(viewModel.BrandId, rw));

            var newWallets = viewModel.ProductWallets.Where(x => x.Id == null || x.Id == Guid.Empty).ToArray();
            newWallets.ForEach(x =>
            {
                var walleTemplate = CreateWalletTemplate(viewModel.BrandId, x.Name, x.ProductIds, false);
                x.Id = walleTemplate.Id;
            });

            _repository.SaveChanges();

            _eventBus.Publish(new WalletTemplateUpdated
            {
                BrandId = viewModel.BrandId,
                RemovedWalletTemplateIds = walletTemplatesToDelete.Select(x => x.Id).ToArray(),
                RemainedWalletTemplates = remainedWallets.Select(x => new WalletTemplateDto
                {
                    Id = x.Id.Value,
                    IsMain = x.IsMain,
                    Name = x.Name,
                    ProductIds = x.ProductIds.ToArray()
                }).ToArray(),
                NewWalletTemplates = newWallets.Select(x => new WalletTemplateDto
                {
                    Id = x.Id.Value,
                    IsMain = x.IsMain,
                    Name = x.Name,
                    ProductIds = x.ProductIds.ToArray()
                }).ToArray(),
                EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
            });
        }
        
        // AZ / UGS-INTEGRATION / AFTREGO-4132
        // Working with multiple wallets are not supported in first release.
        // This operations are required until UGS will not implement multi-wallets functionality
        // It assign all products to main wallet and clears product wallets preventing its creation
        private void TransformToOneWalletModel(WalletTemplateViewModel model)
        {
            if (model.ProductWallets == null)
            {
                return;
            }
            
            model.MainWallet.ProductIds = 
                model.ProductWallets.SelectMany(x => x.ProductIds)
                .Union(model.MainWallet.ProductIds)
                .Distinct()
                .ToList();

            model.ProductWallets.Clear();
        }

        public void MakePaymentLevelDefault(Guid newPaymentLevelId, Guid brandId, string currencyCode)
        {
            var brand = _repository.Brands
                .Include(o => o.BrandCurrencies)
                .Single(o => o.Id == brandId);

            var brandCurrency = brand.BrandCurrencies
                .SingleOrDefault(o => o.CurrencyCode == currencyCode && o.BrandId == brandId);

            if (brandCurrency == null)
                throw new RegoValidationException("{\"text\": \"app:brand.activation.noAssignedCurrency\"}");

            brandCurrency.DefaultPaymentLevelId = newPaymentLevelId;

            _repository.SaveChanges();
        }

        private void UpdateWalletTemplate(Guid brandId, WalletViewModel walletModel)
        {
            var brand = _repository.Brands.Single(x => x.Id == brandId);

            var wallet = _repository.Brands
                .Include(x => x.WalletTemplates.Select(wt => wt.WalletTemplateProducts))
                .Single(x => x.Id == brandId)
                .WalletTemplates
                .Single(wt => wt.Id == walletModel.Id);

            wallet.DateUpdated = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
            wallet.UpdatedBy = _actorInfoProvider.Actor.Id;
            wallet.Name = walletModel.Name;

            var walletTemplateProductsToRemove =
                wallet
                    .WalletTemplateProducts
                    .Where(x => !walletModel.ProductIds.Contains(x.ProductId))
                    .ToList();

            var walletTemplateProductsToAdd =
                walletModel
                    .ProductIds
                    .Where(x => !wallet.WalletTemplateProducts.Select(y => y.ProductId).Contains(x))
                    .ToList();

            if (walletTemplateProductsToRemove.Any() && wallet.Brand.Status == BrandStatus.Active)
                throw new RegoValidationException("It is not allowed to change product wallets for active brands.");

            walletTemplateProductsToRemove.ForEach(product => wallet.WalletTemplateProducts.Remove(product));
            walletTemplateProductsToAdd.ForEach(productId =>
            {
                var walletTemplateProduct = new WalletTemplateProduct
                {
                    Id = Guid.NewGuid(),
                    WalletTemplateId = wallet.Id,
                    WalletTemplate = wallet,
                    ProductId = productId
                };
                wallet.WalletTemplateProducts.Add(walletTemplateProduct);
            });
        }

        [Permission(Permissions.Activate, Module = Modules.VipLevelManager)]
        public void ActivateVipLevel(Guid vipLevelId, string remark)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var vipLevel = _repository.VipLevels
                    .Include(v => v.Brand).Include(v => v.VipLevelGameProviderBetLimits)
                    .Single(v => v.Id == vipLevelId);

                vipLevel.Status = VipLevelStatus.Active;
                vipLevel.UpdatedRemark = remark;
                vipLevel.UpdatedBy = _actorInfoProvider.Actor.UserName;
                vipLevel.DateUpdated = DateTimeOffset.Now;
                _repository.SaveChanges();

                _eventBus.Publish(new VipLevelActivated(vipLevelId, remark)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(vipLevel.Brand.TimezoneId),
                });
                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.VipLevelManager)]
        public void DeactivateVipLevel(Guid deactivateVipLevelId, string remark, Guid? newDefaultVipLevelId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var vipLevelToDeactivate = _repository.VipLevels
                    .Include(v => v.Brand)
                    .Include(v => v.VipLevelGameProviderBetLimits)
                    .Single(v => v.Id == deactivateVipLevelId);

                var isDefaultForItsBrand = vipLevelToDeactivate.Brand.DefaultVipLevelId == vipLevelToDeactivate.Id;

                if (isDefaultForItsBrand && !newDefaultVipLevelId.HasValue)
                    throw new RegoException(
                        "Unable to deactivate default vip level. Please specify new default vip level");

                if (isDefaultForItsBrand)
                {
                    var newVipLevel = _repository.VipLevels
                        .Include(o => o.Brand)
                        .Single(o => o.Id == newDefaultVipLevelId.Value);

                    SetDefaultVipLevel(newVipLevel.Brand, newVipLevel.Id);
                }

                vipLevelToDeactivate.Status = VipLevelStatus.Inactive;
                vipLevelToDeactivate.UpdatedRemark = remark;
                vipLevelToDeactivate.UpdatedBy = _actorInfoProvider.Actor.UserName;
                vipLevelToDeactivate.DateUpdated = DateTimeOffset.Now;
                _repository.SaveChanges();

                _eventBus.Publish(new VipLevelDeactivated(deactivateVipLevelId, remark)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(vipLevelToDeactivate.Brand.TimezoneId),
                });

                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Module = Modules.CountryManager)]
        public void CreateCountry(string code, string name)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var country = new Country
                {
                    Code = code,
                    Name = name
                };
                _repository.Countries.Add(country);
                _repository.SaveChanges();

                _eventBus.Publish(new CountryCreated(country));

                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Module = Modules.CountryManager)]
        public void UpdateCountry(string code, string name)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var country = _repository.Countries.SingleOrDefault(c => c.Code == code);

                if (country == null)
                {
                    throw new RegoException("Country not found");
                }

                country.Name = name;

                _repository.SaveChanges();
                _eventBus.Publish(new CountryUpdated(country));

                scope.Complete();
            }
        }

        [Permission(Permissions.Delete, Module = Modules.CountryManager)]
        public void DeleteCountry(string code)
        {
            var country = _repository.Countries.Single(c => c.Code == code);

            _repository.Countries.Remove(country);
            _repository.SaveChanges();
            _eventBus.Publish(new CountryRemoved(country));
        }
    }
}
