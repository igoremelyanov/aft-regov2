using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.MemberApi;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using VipLevel = AFT.RegoV2.Core.Common.Data.Player.VipLevel;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common.Helpers;
using Bank = AFT.RegoV2.Core.Payment.Data.Bank;
using BankAccount = AFT.RegoV2.Core.Payment.Data.BankAccount;
using BrandCurrency = AFT.RegoV2.Core.Brand.Interface.Data.BrandCurrency;
using Language = AFT.RegoV2.Core.Messaging.Data.Language;
using Licensee = AFT.RegoV2.Core.Brand.Interface.Data.Licensee;
using MessageTemplate = AFT.RegoV2.Core.Messaging.Data.MessageTemplate;
using SecurityQuestion = AFT.RegoV2.Core.Common.Data.SecurityQuestion;
using Currency = AFT.RegoV2.Core.Brand.Interface.Data.Currency;
using Country = AFT.RegoV2.Core.Brand.Interface.Data.Country;

namespace AFT.RegoV2.Tests.Unit.Player
{
    public class TestStartup : Startup
    {
        public new static IUnityContainer Container;

        protected override IUnityContainer GetUnityContainer()
        {
            return Container;
        }
    }

    internal abstract class PlayerServiceTestsBase : AdminWebsiteUnitTestsBase
    {
        protected MemberApiProxy PlayerWebservice { get; set; }
        protected FakeBrandRepository FakeBrandRepository { get; set; }
        protected FakePlayerRepository FakePlayerRepository { get; set; }
        protected FakePaymentRepository FakePaymentRepository { get; set; }
        protected FakeMessagingRepository FakeMessagingRepository { get; set; }
        protected FakeEventRepository FakeEventRepository { get; set; }
        protected FakeSecurityRepository FakeSecurityRepository { get; set; }

        private IDisposable _webServer;

        public override void BeforeEach()
        {
            base.BeforeEach();

            FakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            FakePlayerRepository = Container.Resolve<FakePlayerRepository>();
            FakePaymentRepository = Container.Resolve<FakePaymentRepository>();
            FakeEventRepository = Container.Resolve<FakeEventRepository>();
            FakeSecurityRepository = Container.Resolve<FakeSecurityRepository>();
            FakeMessagingRepository = Container.Resolve<FakeMessagingRepository>();

            for (int i = 0; i < TestDataGenerator.CountryCodes.Length; i++)
            {
                FakeBrandRepository.Countries.Add(new Country { Code = TestDataGenerator.CountryCodes[i] });
            }

            for (int i = 0; i < TestDataGenerator.CurrencyCodes.Length; i++)
            {
                FakeBrandRepository.Currencies.Add(new Currency { Code = TestDataGenerator.CurrencyCodes[i] });
            }

            for (int i = 0; i < TestDataGenerator.CultureCodes.Length; i++)
            {
                FakeBrandRepository.Cultures.Add(new Culture { Code = TestDataGenerator.CultureCodes[i] });
            }

            var brandId = new Guid("00000000-0000-0000-0000-000000000138");
            var brand = new Core.Brand.Interface.Data.Brand { Id = brandId, Name = "138", Status = BrandStatus.Active, TimezoneId = "Pacific Standard Time" };
            for (int i = 0; i < TestDataGenerator.CurrencyCodes.Length; i++)
            {
                var currencyCode = TestDataGenerator.CurrencyCodes[i];

                brand.BrandCurrencies.Add(new BrandCurrency
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CurrencyCode = currencyCode,
                    Currency = FakeBrandRepository.Currencies.Single(x => x.Code == currencyCode),
                    DefaultPaymentLevelId = currencyCode == "CAD"
                        ? new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")
                        : new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9")
                });

            }
            for (int i = 0; i < TestDataGenerator.CountryCodes.Length; i++)
            {
                var countryCode = TestDataGenerator.CountryCodes[i];

                brand.BrandCountries.Add(new BrandCountry
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CountryCode = countryCode,
                    Country = FakeBrandRepository.Countries.Single(x => x.Code == countryCode)
                });
            }
            for (int i = 0; i < TestDataGenerator.CultureCodes.Length; i++)
            {
                var cultureCode = TestDataGenerator.CultureCodes[i];

                brand.BrandCultures.Add(new BrandCulture
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CultureCode = cultureCode,
                    Culture = FakeBrandRepository.Cultures.Single(x => x.Code == cultureCode)
                });
            }
            var walletTemplate = new WalletTemplate()
            {
                Brand = brand,
                Id = Guid.NewGuid(),
                IsMain = true,
                Name = "Main wallet",
                DateCreated = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            brand.WalletTemplates.Add(walletTemplate);
            brand.DefaultCulture = brand.BrandCultures.First().Culture.Code;
            brand.DefaultCurrency = brand.BrandCurrencies.First().Currency.Code;
            var vipLevel = new Core.Brand.Interface.Data.VipLevel { Name = "Standard", BrandId = brandId };
            brand.DefaultVipLevelId = vipLevel.Id;
            brand.DefaultVipLevel = vipLevel;

            FakeBrandRepository.WalletTemplates.Add(walletTemplate);
            var playerVipLevel = new VipLevel
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                BrandId = brandId
            };
            brand.DefaultVipLevelId = playerVipLevel.Id;

            FakeBrandRepository.Brands.Add(brand);
            var playerBrand = new Core.Common.Data.Player.Brand { Id = brand.Id, TimezoneId = brand.TimezoneId };
            FakePlayerRepository.Brands.Add(playerBrand);
            FakePlayerRepository.VipLevels.Add(playerVipLevel);
            
            FakeMessagingRepository.VipLevels.Add(new Core.Messaging.Data.VipLevel
            {
                Id = playerVipLevel.Id,
                Name = playerVipLevel.Name
            });
            
            FakeMessagingRepository.SaveChanges();
            
            playerBrand.DefaultVipLevelId = playerVipLevel.Id;
            FakePlayerRepository.SaveChanges();

            foreach (var questionid in TestDataGenerator.SecurityQuestions)
            {
                FakePlayerRepository.SecurityQuestions.Add(new SecurityQuestion
                {
                    Id = new Guid(questionid),
                    Question = TestDataGenerator.GetRandomString()
                });
            }

            Container.Resolve<FakeGameRepository>().Brands.Add(new Core.Game.Interface.Data.Brand
            {
                Id = new Guid("00000000-0000-0000-0000-000000000138"),
                TimezoneId = TestDataGenerator.GetRandomTimeZone().Id
            });

            var bankAccountType = new BankAccountType
            {
                Id = new Guid("00000000-0000-0000-0000-000000000100"),
                Name = "Main"
            };


            FakePaymentRepository.Brands.Add(new Core.Payment.Data.Brand
            {
                Id = new Guid("00000000-0000-0000-0000-000000000138"),
                TimezoneId = "Pacific Standard Time"
            });

            FakePaymentRepository.BankAccountTypes.Add(bankAccountType);

            var bank = new Bank
            {
                Id = Guid.NewGuid(),
                BankId = "SE45",
                BankName = "Bank of Canada",
                BrandId = brandId,
                CountryCode = "Canada",
                Created = DateTime.Now,
                CreatedBy = "initializer"
            };
            FakePaymentRepository.Banks.Add(bank);

            var cadAccountId = new Guid("B6755CB9-8F9A-4EBA-87E0-1ED5493B7534");
            FakePaymentRepository.BankAccounts.Add(
                new BankAccount
                {
                    Id = cadAccountId,
                    AccountId = "BoC1",
                    AccountName = "John Doe",
                    AccountNumber = "SE45 0583 9825 7466",
                    AccountType = bankAccountType,
                    Bank = bank,
                    Branch = "Main",
                    Province = "Vancouver",
                    CurrencyCode = "CAD",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                }
                );

            bankAccountType = new BankAccountType
            {
                Id = new Guid("00000000-0000-0000-0000-000000000101"),
                Name = "VIP"
            };
            FakePaymentRepository.BankAccountTypes.Add(bankAccountType);

            bank = new Bank
            {
                Id = Guid.NewGuid(),
                BankId = "70AC",
                BankName = "Hua Xia Bank",
                BrandId = brandId,
                CountryCode = "China",
                Created = DateTime.Now,
                CreatedBy = "initializer"
            };
            FakePaymentRepository.Banks.Add(bank);

            var rmbAccountId = new Guid("13672261-70AC-46E3-9E62-9E2E3AB77663");
            FakePaymentRepository.BankAccounts.Add(
                new BankAccount
                {
                    Id = rmbAccountId,
                    AccountId = "HXB1",
                    AccountName = "Beijing",
                    AccountNumber = "BA3912940494",
                    //AccountType = "Main",
                    AccountType = bankAccountType,
                    Bank = bank,
                    Branch = "Main",
                    Province = "Beijing Municipality",
                    CurrencyCode = "RMB",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                }
                );

            var paymentLevel = new PaymentLevel
            {
                Id = new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33"),
                BrandId = brandId,
                CurrencyCode = "CAD",
                Name = "CADLevel",
                Code = "CADLevel",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer"
            };
            paymentLevel.BankAccounts.Add(FakePaymentRepository.BankAccounts.Single(a => a.Id == cadAccountId));
            FakePaymentRepository.PaymentLevels.Add(paymentLevel);

            paymentLevel = new PaymentLevel
            {
                Id = new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9"),
                BrandId = brandId,
                CurrencyCode = "RMB",
                Name = "RMBLevel",
                Code = "RMBLevel",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer"
            };
            paymentLevel.BankAccounts.Add(FakePaymentRepository.BankAccounts.Single(a => a.Id == rmbAccountId));
            FakePaymentRepository.PaymentLevels.Add(paymentLevel);

            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = 1,
                Status = LicenseeStatus.Active
            };

            FakeBrandRepository.Licensees.Add(licensee);
            FakeBrandRepository.SaveChanges();

            foreach (var culture in FakeBrandRepository.Cultures)
            {
                FakeMessagingRepository.Languages.Add(new Language
                {
                    Code = culture.Code,
                    Name = culture.Name
                });
            }

            foreach (var thisBrand in FakeBrandRepository.Brands.Include(x => x.BrandCultures.Select(y => y.Culture)))
            {
                FakeMessagingRepository.Brands.Add(new Core.Messaging.Data.Brand
                {
                    Id = thisBrand.Id,
                    Name = thisBrand.Name,
                    SmsNumber = TestDataGenerator.GetRandomPhoneNumber(),
                    Email = TestDataGenerator.GetRandomEmail(),
                    Languages = thisBrand.BrandCultures.Select(x => new Language
                    {
                        Code = x.Culture.Code,
                        Name = x.Culture.Name
                    }).ToList()
                });
            }

            foreach (var thisPlayer in FakePlayerRepository.Players)
            {
                FakeMessagingRepository.Players.Add(new Core.Messaging.Data.Player
                {
                    Id = thisPlayer.Id,
                    Username = thisPlayer.Username,
                    FirstName = thisPlayer.FirstName,
                    LastName = thisPlayer.LastName,
                    Email = thisPlayer.Email,
                    Language = FakeMessagingRepository.Languages.Single(x => x.Code == thisPlayer.CultureCode),
                    Brand = FakeMessagingRepository.Brands.Single(x => x.Id == thisPlayer.BrandId)
                });
            }

            foreach (var thisBrand in FakeMessagingRepository.Brands.Include(x => x.Languages))
            {
                foreach (var thisLanguage in thisBrand.Languages)
                {
                    foreach (var messageType in (MessageType[])Enum.GetValues(typeof(MessageType)))
                    {
                        FakeMessagingRepository.MessageTemplates.Add(new MessageTemplate
                        {
                            BrandId = thisBrand.Id,
                            LanguageCode = thisLanguage.Code,
                            MessageType = messageType,
                            MessageDeliveryMethod = MessageDeliveryMethod.Email,
                            TemplateName = TestDataGenerator.GetRandomString(),
                            MessageContent = string.Format("Fake email message Template. {0}.",
                                Enum.GetName(typeof (MessageType), messageType)),
                            Subject = TestDataGenerator.GetRandomString(),
                            Status = Status.Active,
                            CreatedBy = "System",
                            Created = DateTimeOffset.UtcNow,
                            ActivatedBy = "System",
                            Activated = DateTimeOffset.UtcNow
                        });

                        FakeMessagingRepository.MessageTemplates.Add(new MessageTemplate
                        {
                            BrandId = thisBrand.Id,
                            LanguageCode = thisLanguage.Code,
                            MessageType = messageType,
                            MessageDeliveryMethod = MessageDeliveryMethod.Sms,
                            TemplateName = TestDataGenerator.GetRandomString(),
                            MessageContent = string.Format("Fake SMS message Template. {0}.",
                                Enum.GetName(typeof(MessageType), messageType)),
                            Status = Status.Active,
                            CreatedBy = "System",
                            Created = DateTimeOffset.UtcNow,
                            ActivatedBy = "System",
                            Activated = DateTimeOffset.UtcNow
                        });
                    }
                }
            }

            FakeMessagingRepository.SaveChanges();

            var securityHelper = Container.Resolve<SecurityTestHelper>();
            securityHelper.PopulatePermissions();

            var licenseeIds = new[] { licensee.Id };
            var brandIds = new[] { brand.Id };

            const string superAdminUsername = "SuperAdmin";

            var adminId = RoleIds.SuperAdminId;
            var role = new Role
            {
                Id = adminId,
                Code = "SuperAdmin",
                Name = "SuperAdmin",
                CreatedDate = DateTime.UtcNow
            };

            role.SetLicensees(licenseeIds);

            var user = new Core.Security.Data.Users.Admin
            {
                Id = adminId,
                Username = superAdminUsername,
                FirstName = superAdminUsername,
                LastName = superAdminUsername,
                IsActive = true,
                Description = superAdminUsername,
                Role = role
            };
            user.SetLicensees(licenseeIds);

            foreach (var licenseeId in licenseeIds)
            {
                user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    AdminId = user.Id,
                    LicenseeId = licenseeId,
                    Admin = user
                });
            }

            user.SetAllowedBrands(brandIds);

            foreach (var item in brandIds)
            {
                user.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    AdminId = user.Id,
                    BrandId = item,
                    Admin = user
                });
            }

            FakeSecurityRepository.Admins.AddOrUpdate(user);
            var authCommands = Container.Resolve<IAuthCommands>();
            authCommands.CreateRole(new CreateRole
            {
                RoleId = adminId,
                Permissions = Container.Resolve<IAuthQueries>().GetPermissions().Select(p => p.Id).ToList()
            });
            authCommands.CreateActor(new CreateActor
            {
                ActorId = adminId,
                Username = superAdminUsername,
                Password = superAdminUsername
            });
            authCommands.AssignRoleToActor(new AssignRole
            {
                ActorId = adminId,
                RoleId = adminId
            });

            FakeSecurityRepository.SaveChanges();

            securityHelper.SignInAdmin(user);

            var testServerUri = ConfigurationManager.AppSettings["TestServerUri"];
            TestStartup.Container = Container;
            _webServer = WebApp.Start<TestStartup>(testServerUri);

            PlayerWebservice = new MemberApiProxy(testServerUri);
        }

        protected async Task<RegisterRequest> RegisterPlayer(bool doLogin = true)
        {
            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();

            await PlayerWebservice.RegisterAsync(registrationData);

            if (doLogin)
            {
                await PlayerWebservice.Login(new LoginRequest
                {
                    Username = registrationData.Username,
                    Password = registrationData.Password,
                    BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                    IPAddress = registrationData.IpAddress,
                    RequestHeaders = new Dictionary<string, string>()
                });
            }
            return registrationData;
        }

        public override void AfterEach()
        {
            _webServer.Dispose();
        }
    }
}