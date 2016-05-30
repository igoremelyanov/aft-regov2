using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using RiskLevel = AFT.RegoV2.Core.Fraud.Interface.Data.RiskLevel;
using RiskLevelStatus = AFT.RegoV2.Core.Common.Data.RiskLevelStatus;

namespace AFT.RegoV2.Infrastructure.DataAccess
{
    public partial class ApplicationSeeder
    {
        private void AddLicensee(Guid licenseeId, string name, string companyName, string email)
        {
            if (_brandRepository.Licensees.Any(b => b.Name == name))
                return;

            var contractStartDate = DateTimeOffset.ParseExact(
                DateTimeOffset.UtcNow.ToString("yyyy'/'MM'/'dd"),
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal
            );

            _licenseeCommands.Add(new AddLicenseeData
            {
                Id = licenseeId,
                Name = name,
                CompanyName = companyName,
                Email = email,
                ContractStart = contractStartDate,
                ContractEnd = contractStartDate.AddMonths(1),
                TimeZoneId = "Pacific Standard Time",
                BrandCount = 100,
                WebsiteCount = 10,

                // Products = new[] { Guid.NewGuid().ToString() },
                Countries = _brandRepository.Countries.Select(c => c.Code).ToArray(),
                Currencies = _brandRepository.Currencies.Select(c => c.Code).ToArray(),
                Languages = _brandRepository.Cultures.Select(c => c.Code).ToArray()
            });

            _licenseeCommands.Activate(licenseeId, "Activated when database has been seeded on first application start");
        }

        private void AddBrand(Guid brandId, Guid licenseeId, string name, string code, string timeZoneId, PlayerActivationMethod playerActivationMethod,
            string email, string smsNumber, string websiteUrl)
        {
            if (_brandRepository.Brands.Any(b => b.Id == brandId))
                return;

            _brandCommands.AddBrand(new AddBrandRequest
            {
                Id = brandId,
                Licensee = licenseeId,
                Type = BrandType.Deposit,
                Name = name,
                Code = code,
                Email = email,
                SmsNumber = smsNumber,
                WebsiteUrl = websiteUrl,
                PlayerActivationMethod = playerActivationMethod,
                TimeZoneId = timeZoneId
            });
        }

        private void AssignBrandCultures(Guid brandId, string[] cultures, string defaultCulture)
        {
            var brand = _brandRepository.Brands.Include(x => x.BrandCultures).FirstOrDefault(b => b.Id == brandId);

            if (brand == null || brand.DefaultCulture == defaultCulture && brand.BrandCultures.Select(x => x.CultureCode).ScrambledEquals(cultures))
            {
                return;
            }

            _brandCommands.AssignBrandCulture(new AssignBrandCultureRequest()
            {
                Brand = brandId,
                DefaultCulture = defaultCulture,
                Cultures = cultures
            });
        }

        private void AssignBrandCountries(Guid brandId, string[] countries)
        {
            var brand = _brandRepository.Brands.Include(x => x.BrandCountries).FirstOrDefault(b => b.Id == brandId);

            if (brand == null || brand.BrandCountries.Select(x => x.CountryCode).ScrambledEquals(countries))
            {
                return;
            }

            _brandCommands.AssignBrandCountry(new AssignBrandCountryRequest
            {
                Brand = brandId,
                Countries = countries
            });
        }

        private void AssignBrandCurrencies(Guid brandId, string[] currencies, string defaultCurrency, string baseCurrency)
        {
            var brand = _brandRepository.Brands.Include(x => x.BrandCurrencies).FirstOrDefault(b => b.Id == brandId);

            if (brand == null || brand.DefaultCurrency == defaultCurrency && brand.BaseCurrency == baseCurrency &&
                brand.BrandCurrencies.Select(x => x.CurrencyCode).ScrambledEquals(currencies))
            {
                return;
            }

            _brandCommands.AssignBrandCurrency(new AssignBrandCurrencyRequest()
            {
                Brand = brandId,
                BaseCurrency = baseCurrency,
                DefaultCurrency = defaultCurrency,
                Currencies = currencies
            });
        }

        private void ActivateBrand(Guid brandId)
        {
            var brand = _brandRepository.Brands.FirstOrDefault(b => b.Id == brandId);
            if (brand == null)
            {
                return;
            }

            if (brand.Status != BrandStatus.Active)
            {
                _brandCommands.ActivateBrand(new ActivateBrandRequest()
                {
                    BrandId = brandId,
                    Remarks = "Activated when database has been seeded on first application start",
                });
            }
        }

        public void AddVipLevel(Guid vipLevelId, Guid brandId, string code, string name, string description, string colorCode, int rank, bool isDefault)
        {
            if (_brandRepository.VipLevels.Any(v => v.Id == vipLevelId))
                return;

            _brandCommands.AddVipLevel(new VipLevelViewModel
            {
                Id = vipLevelId,
                Brand = brandId,
                Code = code,
                Name = name,
                Description = description,
                Color = colorCode,
                Rank = rank,
                IsDefault = isDefault
            });

            _brandCommands.ActivateVipLevel(vipLevelId, "Activated when database has been seeded on first application start");
        }

        public void AddRiskLevel(Guid id, Guid brandId, string name, int level, bool status)
        {
            if (_brandRepository.RiskLevels.Any(x => x.Id == id))
                return;

            _riskLevelCommands.Create(new RiskLevel
            {
                Id = id,
                BrandId = brandId,
                Name = name,
                Level = level,
                Status = status ? RiskLevelStatus.Active : RiskLevelStatus.Inactive,
                Description = "Created automatically while seeding database at first start"
            });
        }

        private void CreateBrandsWalletStructure(Guid licenseeId, Guid brand138Id, Guid brand831Id, Guid providerMockGpId, Guid providerMockSbId)
        {
            var brand138IdCount = _brandRepository.WalletTemplates.Count(x => x.BrandId == brand138Id);
            if (brand138IdCount == 0)
            {
                _brandCommands.CreateWalletStructureForBrand(new WalletTemplateViewModel
                {
                    BrandId = brand138Id,
                    LicenseeId = licenseeId,
                    MainWallet = new WalletViewModel
                    {
                        IsMain = true,
                        Name = "Main 138",
                        ProductIds = new[] { providerMockSbId }
                    },
                    ProductWallets = new List<WalletViewModel>
                    {
                        new WalletViewModel
                        {
                            IsMain = false,
                            Name = "Product 138",
                            ProductIds = new[] { providerMockGpId }
                        }
                    }
                });
            }

            var brand831IdCount = _brandRepository.WalletTemplates.Count(x => x.BrandId == brand831Id);
            if (brand831IdCount == 0)
            {
                _brandCommands.CreateWalletStructureForBrand(new WalletTemplateViewModel
                {
                    BrandId = brand831Id,
                    LicenseeId = licenseeId,
                    MainWallet = new WalletViewModel
                    {
                        IsMain = true,
                        Name = "Main 831",
                        ProductIds = new[] { providerMockGpId }
                    }
                });
            }
        }

        private void AssignLicenseeProducts(Guid licenseeId, params Guid[] productIds)
        {
            var licensee = _brandRepository.Licensees
                .Include(x => x.Countries)
                .Include(x => x.Currencies)
                .Include(x => x.Cultures)
                .Include(x => x.Products)
                .FirstOrDefault(x => x.Id == licenseeId);
            if (licensee == null)
                return;

            if (licensee.Products.Select(x => x.ProductId).ToArray().ScrambledEquals(productIds))
            {
                return;
            }

            _licenseeCommands.Edit(new EditLicenseeData
            {
                Id = licensee.Id,
                Name = licensee.Name,
                CompanyName = licensee.CompanyName,
                Email = licensee.Email,
                ContractStart = licensee.ContractStart,
                ContractEnd = licensee.ContractEnd,
                TimeZoneId = licensee.TimezoneId,
                BrandCount = licensee.AllowedBrandCount,
                WebsiteCount = licensee.AllowedWebsiteCount,
                Remarks = string.IsNullOrEmpty(licensee.Remarks) ? "Changed when assigning new products on initial seeding" : licensee.Remarks,
                Countries = licensee.Countries.Select(c => c.Code).ToArray(),
                Currencies = licensee.Currencies.Select(c => c.Code).ToArray(),
                Languages = licensee.Cultures.Select(c => c.Code).ToArray(),
                OpenEnded = licensee.ContractEnd.HasValue,
                AffiliateSystem = licensee.AffiliateSystem,
                Products = productIds.Select(x => x.ToString()).ToArray()
            });
        }

        private void AssignBrandProducts(Guid brandId, params Guid[] productIds)
        {
            var brand = _brandRepository.Brands.Include(x => x.Products).FirstOrDefault(x => x.Id == brandId);
            if (brand == null)
                return;

            if (brand.Products.Select(x => x.ProductId).ToArray().ScrambledEquals(productIds))
            {
                return;
            }

            _brandCommands.AssignBrandProducts(new AssignBrandProductsData
            {
                BrandId = brandId,
                ProductsIds = productIds
            });
        }

        public void AssignBrandCredentials(Guid brandId, string clientId, string clientSecret)
        {
            var brand = _gameRepository.Brands.FirstOrDefault(x => x.Id == brandId);
            if (brand == null)
                return;

            _gameManagement.AssignBrandCredentials(new BrandCredentialsData(brandId, clientId, clientSecret));
        }
    }
}
