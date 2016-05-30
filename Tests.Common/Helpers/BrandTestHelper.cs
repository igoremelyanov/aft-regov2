using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Shared.Utils;
using Brand = AFT.RegoV2.Core.Brand.Interface.Data.Brand;
using Country = AFT.RegoV2.Core.Brand.Interface.Data.Country;
using Culture = AFT.RegoV2.Core.Common.Brand.Data.Culture;
using Currency = AFT.RegoV2.Core.Brand.Interface.Data.Currency;
using Licensee = AFT.RegoV2.Core.Brand.Interface.Data.Licensee;
using RiskLevel = AFT.RegoV2.Core.Fraud.Interface.Data.RiskLevel;
using VipLevel = AFT.RegoV2.Core.Brand.Interface.Data.VipLevel;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class BrandTestHelper
    {
        private readonly IBrandRepository _brandRepository;
        private readonly BrandCommands _brandCommands;
        private readonly LicenseeCommands _licenseeCommands;
        private readonly BrandQueries _brandQueries;
        private readonly GamesTestHelper _gamesTestHelper;
        private readonly PaymentTestHelper _paymentTestHelper;
        private readonly ICultureCommands _cultureCommands;
        private readonly RiskLevelCommands _riskLevelCommands;
        private readonly IGameManagement _gameManagement;

        public BrandTestHelper(
            IBrandRepository brandRepository,
            BrandCommands brandCommands,
            LicenseeCommands licenseeCommands,
            BrandQueries brandQueries,
            GamesTestHelper gamesTestHelper,
            PaymentTestHelper paymentTestHelper,
            ICultureCommands cultureCommands,
            RiskLevelCommands riskLevelCommands,
            IGameManagement gameManagement)
        {
            _brandRepository = brandRepository;
            _brandCommands = brandCommands;
            _licenseeCommands = licenseeCommands;
            _brandQueries = brandQueries;
            _gamesTestHelper = gamesTestHelper;
            _paymentTestHelper = paymentTestHelper;
            _cultureCommands = cultureCommands;
            _riskLevelCommands = riskLevelCommands;
            _gameManagement = gameManagement;
        }

        public Brand CreateBrand(
            Licensee licensee = null,
            Country country = null,
            Culture culture = null,
            Currency currency = null,
            bool isActive = false)
        {

            licensee = licensee ?? _brandRepository.Licensees.FirstOrDefault() ?? CreateLicensee();

            var products = licensee.Products;

            country = country ?? licensee.Countries.First();
            culture = culture ?? licensee.Cultures.First();
            currency = currency ?? licensee.Currencies.First();
            
            var brandId = CreateBrand(licensee, PlayerActivationMethod.Automatic);
            CreateWallet(licensee.Id, brandId, products.Select(x => x.ProductId).ToArray());
            AssignCountry(brandId, country.Code);
            AssignCurrency(brandId, currency.Code);
            _brandRepository.SaveChanges();
            AssignCulture(brandId, culture.Code);
            AssignProducts(brandId, products.Select(x => x.ProductId).ToArray());
            CreateRiskLevel(brandId);
            AssignBrandCredentials(brandId);

            _paymentTestHelper.CreateBank(brandId, country.Code);
            _paymentTestHelper.CreateBankAccount(brandId, currency.Code);
            _paymentTestHelper.CreatePaymentGatewaySettings(brandId);
            _paymentTestHelper.CreatePaymentLevel(brandId, currency.Code);

            CreateVipLevel(brandId);

            if (isActive)
                _brandCommands.ActivateBrand(new ActivateBrandRequest
                {
                    BrandId = brandId,
                    Remarks = TestDataGenerator.GetRandomString()
                });

            return _brandQueries.GetBrandOrNull(brandId);
        }

        public Guid CreateBrand(Licensee licensee, PlayerActivationMethod playerActivationMethod)
        {
            var brandId = _brandCommands.AddBrand(new AddBrandRequest
            {
                Code = TestDataGenerator.GetRandomString(),
                InternalAccounts = 1,
                EnablePlayerPrefix = true,
                PlayerPrefix = TestDataGenerator.GetRandomString(3),
                Licensee = licensee.Id,
                Name = TestDataGenerator.GetRandomString(),
                Email = TestDataGenerator.GetRandomEmail(),
                SmsNumber = TestDataGenerator.GetRandomPhoneNumber().Replace("-", string.Empty),
                WebsiteUrl = TestDataGenerator.GetRandomWebsiteUrl(),
                PlayerActivationMethod = playerActivationMethod,
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Type = BrandType.Integrated
            });

            licensee.Brands.Add(_brandRepository.Brands.Single(b => b.Id == brandId));

            return brandId;
        }

        public Licensee CreateLicensee(bool isActive = true, IEnumerable<Culture> cultures = null, IEnumerable<Country> countries = null, IEnumerable<Currency> currencies = null, string [] productIds = null )
        {
            countries = countries ?? new List<Country> { CreateCountry("CA", "Canada") };
            cultures = cultures ?? new List<Culture> { CreateCulture("en-CA", "English (Canada)") };
            currencies = currencies ?? new List<Currency> { CreateCurrency("CAD", "Canadian Dollar") };

            if (productIds == null)
            {
                var product1 = _gamesTestHelper.CreateGameProvider();
                var product2 = _gamesTestHelper.CreateGameProvider();
                productIds = new [] { product1.Id.ToString(), product2.Id.ToString() };
            }

            var licenseeId = _licenseeCommands.Add(new AddLicenseeData
            {
                BrandCount = 10,
                Name = TestDataGenerator.GetRandomString(),
                CompanyName = TestDataGenerator.GetRandomString(),
                Email = TestDataGenerator.GetRandomEmail(),
                ContractStart = DateTime.UtcNow,
                ContractEnd = DateTime.UtcNow.AddMonths(1),
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Products = productIds ,
                Countries = countries.Select(c => c.Code).ToArray(),
                Currencies = currencies.Select(c => c.Code).ToArray(),
                Languages = cultures.Select(c => c.Code).ToArray()
            });

            if (isActive)
                _licenseeCommands.Activate(licenseeId, TestDataGenerator.GetRandomString());

            return _brandQueries.GetLicensee(licenseeId);
        }

        public void CreateWallet(Guid licenseeId, Guid brandId, IEnumerable<Guid> productIds = null)
        {
            IEnumerable<Guid> mainWalletProductIds = null;

            if (productIds != null)
            {
                var productList = productIds.ToList();
                if (productList.Count > 1)
                {
                    mainWalletProductIds = new List<Guid> {productList.First()};
                    productList.RemoveAt(0);
                    productIds = productList;
                }
            }
            _brandCommands.CreateWalletStructureForBrand(new WalletTemplateViewModel
            {
                BrandId = brandId,
                LicenseeId = licenseeId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main",
                    IsMain = true,
                    ProductIds = mainWalletProductIds ?? new List<Guid> { Guid.Empty }
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "Product",
                        IsMain = false,
                        ProductIds = productIds ?? new List<Guid>{Guid.Empty}
                    }
                }
            });

            //Unit tests brand to wallet template persistence fix
            var brand = _brandRepository.Brands.Single(b => b.Id == brandId);
            if (brand != null)
                brand.WalletTemplates.ForEach(wt =>
                {
                    wt.Brand = brand;
                    _brandRepository.WalletTemplates.Add(wt);
                });
        }

        public Currency CreateCurrency(string code, string name)
        {
            var currency = _brandRepository.Currencies.SingleOrDefault(c => c.Code == code);
            if (currency == null)
            {
                currency = new Currency
                {
                    Code = code,
                    Name = name
                };
                _brandRepository.Currencies.Add(currency);
            }

            return currency;
        }

        public void AssignLicenseeCurrency(Guid licenseeId, string code)
        {
            var licensee = _brandRepository.Licensees
                .Include(x => x.Currencies)
                .Single(x => x.Id == licenseeId);

            if (licensee.Currencies.All(x => x.Code != code))
                licensee.Currencies.Add(_brandRepository.Currencies.Single(x => x.Code == code));
            
            _brandRepository.SaveChanges();
        }

        public void AssignCurrency(Guid brandId, string code)
        {
            _brandCommands.AssignBrandCurrency(new AssignBrandCurrencyRequest
            {
                Brand = brandId,
                Currencies = new[] { code },
                DefaultCurrency = code,
                BaseCurrency = code,
            });
            _brandRepository.SaveChanges();
        }

        public void AssignProducts(Guid brandId, IEnumerable<Guid> productIds)
        {
            _brandCommands.AssignBrandProducts(new AssignBrandProductsData
            {
                BrandId = brandId,
                ProductsIds = productIds.ToArray()
            });
        }

        public Culture CreateCulture(string code, string name)
        {
            var culture = _brandRepository.Cultures.SingleOrDefault(c => c.Code == code);
            if (culture == null)
            {
                _cultureCommands.Save(new EditCultureData
                {
                    Code = code,
                    Name = name,
                    NativeName = name
                });
                _brandCommands.ActivateCulture(code, "remark");
                culture = _brandRepository.Cultures.Single(c => c.Code == code);
            }
            return culture;
        }

        public void AssignCulture(Guid brandId, string cultureCode)
        {
            _brandCommands.AssignBrandCulture(new AssignBrandCultureRequest
            {
                Brand = brandId,
                Cultures = new[] { cultureCode },
                DefaultCulture = cultureCode
            });
        }

        public Country CreateCountry(string code, string name)
        {
            var country = _brandRepository.Countries.SingleOrDefault(c => c.Code == code);
            if (country == null)
            {
                _brandCommands.CreateCountry(code, name);
                country = _brandRepository.Countries.Single(c => c.Code == code);
            }

            return country;
        }

        public void AssignCountry(Guid brandId, string code)
        {
            _brandCommands.AssignBrandCountry(new AssignBrandCountryRequest
            {
                Brand = brandId,
                Countries = new[] { code }
            });
        }

        public VipLevel CreateVipLevel(Guid brandId, int limitCount = 0, bool isDefault = true)
        {
            var brand = _brandRepository.Brands.Single(x => x.Id == brandId);
            var vipLevelName = TestDataGenerator.GetRandomString(12);
            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(1000);
            } while (_brandRepository.VipLevels.Any(vl => vl.Rank == rank));
            var limits = new List<VipLevelLimitViewModel>();
            for (var i = 0; i < limitCount; i++)
            {
                var gameProvider = _gamesTestHelper.CreateGameProvider();
                var betLimit = _gamesTestHelper.CreateBetLevel(gameProvider, brand.Id);
                limits.Add(new VipLevelLimitViewModel
                {
                    Id = Guid.NewGuid(),
                    CurrencyCode = brand.DefaultCurrency,
                    GameProviderId = gameProvider.Id,
                    BetLimitId = betLimit.Id
                });
            }

            var newVipLevel = new VipLevelViewModel
            {
                Name = vipLevelName,
                Code = TestDataGenerator.GetRandomString(10),
                Brand = brand.Id,
                Rank = rank,
                Limits = limits,
                IsDefault = isDefault
            };
            _brandCommands.AddVipLevel(newVipLevel);
            var vipLevel = _brandQueries.GetVipLevels().Single(x => x.Code == newVipLevel.Code);

            return vipLevel;
        }


        public VipLevel CreateNotDefaultVipLevel(Guid brandId, int limitCount = 0, bool isDefault = false)
        {
            var brand = _brandRepository.Brands.Single(x => x.Id == brandId);
            var vipLevelName = TestDataGenerator.GetRandomString();
            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(100);
            } while (_brandRepository.VipLevels.Any(vl => vl.Rank == rank));
            var limits = new List<VipLevelLimitViewModel>();
            for (var i = 0; i < limitCount; i++)
            {
                var gameProvider = _gamesTestHelper.CreateGameProvider();
                var betLimit = _gamesTestHelper.CreateBetLevel(gameProvider, brand.Id);
                limits.Add(new VipLevelLimitViewModel
                {
                    Id = Guid.NewGuid(),
                    CurrencyCode = brand.DefaultCurrency,
                    GameProviderId = gameProvider.Id,
                    BetLimitId = betLimit.Id
                });
            }

            var newVipLevel = new VipLevelViewModel
            {
                Name = vipLevelName,
                Code = vipLevelName.Remove(3),
                Brand = brand.Id,
                Rank = rank,
                Limits = limits,
                IsDefault = isDefault
            };
            _brandCommands.AddVipLevel(newVipLevel);
            var vipLevel = _brandQueries.GetVipLevels().Single(x => x.Code == newVipLevel.Code);

            return vipLevel;
        }

        public Guid CreateRiskLevel(Guid brandId, bool activate = true)
        {
            int level;
            do
            {
                level = TestDataGenerator.GetRandomNumber(1000);
            } while (_brandRepository.RiskLevels.Any(rl => rl.BrandId==brandId&&rl.Level == level));

            var riskLevel = new RiskLevel
            {
                BrandId = brandId,
                Level = level,
                Name = TestDataGenerator.GetRandomAlphabeticString(10),
                Description = TestDataGenerator.GetRandomAlphabeticString(10)
            };
            _riskLevelCommands.Create(riskLevel);
            if (activate)
            {
                _riskLevelCommands.Activate(riskLevel.Id, TestDataGenerator.GetRandomString());
            }

            return riskLevel.Id;
        }

        public void AssignBrandCredentials(Guid brandId, string clientId = "", string clientSecret = "")
        {
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = TestDataGenerator.GetRandomAlphabeticString(10);
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                clientId = TestDataGenerator.GetRandomAlphabeticString(44);
            }

            _gameManagement.AssignBrandCredentials(new BrandCredentialsData(brandId, clientId, clientSecret));
        }

        public Licensee GetDefaultLicensee()
        {
            var licensee = _brandQueries.GetLicensees().First(x => x.Name == "Flycow");
            return licensee;
        }

        public Brand CreateActiveBrandWithProducts()
        {
            var mainWalletProduct = _gamesTestHelper.CreateGameProvider();
            var productWalletProduct = _gamesTestHelper.CreateGameProvider();

            var licensee = CreateLicensee(productIds: new[]
            {
                mainWalletProduct.Id.ToString(),
                productWalletProduct.Id.ToString()
            });

            return CreateBrand(licensee, isActive: true);
        }
    }
}