using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentValidation;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Bank = AFT.RegoV2.Core.Payment.Data.Bank;
using BankAccount = AFT.RegoV2.Core.Payment.Data.BankAccount;
using BrandCurrency = AFT.RegoV2.Core.Brand.Interface.Data.BrandCurrency;
using Country = AFT.RegoV2.Core.Brand.Interface.Data.Country;
using Currency = AFT.RegoV2.Core.Brand.Interface.Data.Currency;
using Language = AFT.RegoV2.Core.Messaging.Data.Language;
using Licensee = AFT.RegoV2.Core.Brand.Interface.Data.Licensee;
using MessageTemplate = AFT.RegoV2.Core.Messaging.Data.MessageTemplate;
using PaymentLevel = AFT.RegoV2.Core.Payment.Data.PaymentLevel;
using SecurityQuestion = AFT.RegoV2.Core.Common.Data.SecurityQuestion;
using VipLevel = AFT.RegoV2.Core.Common.Data.Player.VipLevel;

namespace AFT.RegoV2.Tests.Unit.Player
{
    using TestPair = Pair<Expression<Func<RegistrationData, object>>, object>;

    class RegistrationTests: UnitTestsBase
    {
        protected FakeBrandRepository FakeBrandRepository { get; set; }
        protected FakePlayerRepository FakePlayerRepository { get; set; }
        protected FakePaymentRepository FakePaymentRepository { get; set; }
        protected FakeMessagingRepository FakeMessagingRepository { get; set; }
        protected FakeSecurityRepository FakeSecurityRepository { get; set; }
        public PlayerCommands PlayerCommands { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            FakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            FakePlayerRepository = Container.Resolve<FakePlayerRepository>();
            FakePaymentRepository = Container.Resolve<FakePaymentRepository>();
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
                                Enum.GetName(typeof(MessageType), messageType)),
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

            PlayerCommands = Container.Resolve<PlayerCommands>();
        }

        [Test]
        public void Can_register_Player()
        {
            var registrationData = TestDataGenerator.CreateRandomRegistrationData();

            PlayerCommands.Register(registrationData);
        }

        public static TestPair[] GetRequiredFields()
        {
            return new[]
            {
                new TestPair(r => r.Username, null),
                new TestPair(r => r.Password, null),
                new TestPair(r => r.FirstName, null),
                new TestPair(r => r.LastName, null),
                new TestPair(r => r.MailingAddressLine1, null),
                new TestPair(r => r.MailingAddressPostalCode, null),
                new TestPair(r => r.CountryCode, null),
                new TestPair(r => r.CurrencyCode, null),
                new TestPair(r => r.Email, null),
                new TestPair(r => r.PhoneNumber, null),
                new TestPair(r => r.DateOfBirth, null),
                new TestPair(r => r.Gender, null),
                new TestPair(r => r.Title, null),
                new TestPair(r => r.MailingAddressCity, null),
                new TestPair(r => r.ContactPreference, null),
                new TestPair(r => r.Username, string.Empty),
                new TestPair(r => r.Password, string.Empty),
                new TestPair(r => r.FirstName, string.Empty),
                new TestPair(r => r.LastName, string.Empty),
                new TestPair(r => r.MailingAddressLine1, string.Empty),
                new TestPair(r => r.MailingAddressPostalCode, string.Empty),
                new TestPair(r => r.CountryCode, string.Empty),
                new TestPair(r => r.CurrencyCode, string.Empty),
                new TestPair(r => r.Email, string.Empty),
                new TestPair(r => r.PhoneNumber, string.Empty),
                new TestPair(r => r.DateOfBirth, string.Empty),
                new TestPair(r => r.Gender, string.Empty),
                new TestPair(r => r.Title, string.Empty),
                new TestPair(r => r.MailingAddressCity, string.Empty),
                new TestPair(r => r.ContactPreference, string.Empty),
                new TestPair( r=> r.SecurityQuestionId, string.Empty),
                new TestPair( r => r.SecurityAnswer, string.Empty),
            };
        }

        public static TestPair[] GetAllowedRangeFields()
        {
            var str = string.Empty;
            return new[]
            {
                new TestPair(r => r.Username, str.PadRight(5, 'a')),
                new TestPair(r => r.Username, str.PadRight(13, 'b')),

                new TestPair(r => r.Email, "@test.com".PadLeft(51, 'c')),
                new TestPair(r => r.Email, "ewrewerwerwerwer"),

                new TestPair(r => r.Password, str.PadRight(5, 'd')),
                new TestPair(r => r.Password, str.PadRight(13, 'd')),

                new TestPair(r => r.FirstName, str.PadLeft(51, 'e')),
                new TestPair(r => r.FirstName, "werwerwerwerw  werwerw"),
                new TestPair(r => r.FirstName, " werwerwerwerw werwerw"),
                new TestPair(r => r.FirstName, "werwerwerwerw werwerw "),
                new TestPair(r => r.FirstName, "werwerwerwerw+"),

                new TestPair(r => r.LastName, str.PadLeft(21, 'f')),
                new TestPair(r => r.LastName, "werweerwerw  werwer"),
                new TestPair(r => r.LastName, " werwerrwerw werwerw"),
                new TestPair(r => r.LastName, "werrwerwerw werwerw "),
                new TestPair(r => r.LastName, "werwerwerwerw+"),

                new TestPair(r => r.PhoneNumber, str.PadRight(7, '1')),
                new TestPair(r => r.PhoneNumber, str.PadRight(16, '1')),

                new TestPair(r => r.DateOfBirth, "111 222 11 22"),
                new TestPair(r => r.DateOfBirth, "01/13/2014"),
                new TestPair(r => r.DateOfBirth, "12/12/2014"),

                new TestPair(r => r.MailingAddressLine1, str.PadLeft(51, 'g')),
                new TestPair(r => r.CountryCode, "RU"),
                new TestPair(r => r.CurrencyCode, "RUB"),

                new TestPair(r => r.Email, "Abc.example.com"),
                new TestPair(r => r.Email, "A@b@c@example.com"),

                new TestPair(r => r.Email, @"a""b(c)d,e:f;g<h>i[j\k]l@example.com"),
                new TestPair(r => r.Email, @"just""not""right@example.com"),
                new TestPair(r => r.Email, @"this is""not\allowed@example.com"),
                new TestPair(r => r.Email, @"this\ still\""notallowed@example.com"),

                new TestPair(r => r.MailingAddressPostalCode, str),
                new TestPair(r => r.MailingAddressPostalCode, str.PadRight(11, '1')),

                new TestPair(r => r.MailingAddressCity, str),
                new TestPair(r => r.ContactPreference, str),

                new TestPair(r => r.SecurityQuestionId, str),
                new TestPair( r => r.SecurityAnswer, str),

            };
        }

        public static TestPair[] GetValidCases()
        {
            var str = string.Empty;
            return new[]
            {
                new TestPair(r => r.Username, str.PadRight(6, 'a')),

                new TestPair(r => r.Username, "q-wert-y"),
                new TestPair(r => r.Username, "qw'er'ty"),
                new TestPair(r => r.Username, "q_we_rty"),
                new TestPair(r => r.Username, "qwe.rt.y"),

                new TestPair(r => r.Email, "test@test.com"),

                new TestPair(r => r.Password, str.PadRight(6, 'd')),

                new TestPair(r => r.FirstName, str.PadLeft(5, 'e')),

                new TestPair(r => r.FirstName, "q-wert-y"),
                new TestPair(r => r.FirstName, "qw'er'ty"),
                new TestPair(r => r.FirstName, "qwe.rt.y"),

                new TestPair(r => r.LastName, str.PadLeft(5, 'f')),

                new TestPair(r => r.LastName, "q-wert-y"),
                new TestPair(r => r.LastName, "qw'er'ty"),
                new TestPair(r => r.LastName, "qwe.rt.y"),

                new TestPair(r => r.MailingAddressPostalCode, "ret 33"),
                new TestPair(r => r.MailingAddressLine1, "ret 33 444 433"),

                new TestPair(r => r.Email, "niceandsimple@example.com"),
                new TestPair(r => r.Email, "very.common@example.com"),
                new TestPair(r => r.Email, "a.little.lengthy.but.fine@dept.example.com"),
                new TestPair(r => r.Email, "disposable.style.email.with+symbol@example.com"),
                new TestPair(r => r.Email, "other.email-with-dash@example.com"),
                new TestPair(r => r.Email, "!#$%&'*+-/=?^_`{}|~@example.org"),

                new TestPair(r => r.Gender, "Male"),
                new TestPair( r=> r.Title, "Mr"),
                new TestPair( r => r.MailingAddressLine2, "address line 2"),
                new TestPair( r => r.MailingAddressLine3, "address line 3"),
                new TestPair( r => r.MailingAddressLine4, "address line 4"),
                new TestPair( r => r.MailingAddressCity, "Singapore"),
                new TestPair( r => r.ContactPreference, "Email")
            };
        }

        [Test]
        [TestCaseSource(nameof(GetRequiredFields))]
        public void Player_regisration_fails_when_required_field_is_missing(TestPair propertyValuePair)
        {
            ValidateTestCase(propertyValuePair);
        }

        [Test]
        [TestCaseSource(nameof(GetAllowedRangeFields))]
        public void Player_registration_fails_when_fields_not_within_its_allowed_range(TestPair propertyValuePair)
        {
            ValidateTestCase(propertyValuePair);
        }

        [Test]
        [TestCaseSource(nameof(GetValidCases))]
        public void Player_registration_should_not_fail_when_fields_are_valid(TestPair propertyValuePair)
        {
            ValidateTestCase(propertyValuePair, true);
        }

        [Test]
        public void Player_registration_fails_when_username_already_exists()
        {
            const string userName = "user_name";
            var brand139 = new Core.Brand.Interface.Data.Brand { Id = Guid.NewGuid(), Name = "139" };
            FakePlayerRepository.Players.Add(new Core.Common.Data.Player.Player { Username = userName, BrandId = brand139.Id });

            var testCase = new TestPair(r => r.Username, userName);
            ValidateTestCase(testCase, true);

            FakeBrandRepository.Brands.Add(brand139);
            ValidateTestCase(testCase);
        }

        [Test]
        public void Player_registration_fails_when_email_already_exists()
        {
            const string email = "test@test.com";

            var brand139 = new Core.Brand.Interface.Data.Brand { Id = Guid.NewGuid(), Name = "139" };
            FakePlayerRepository.Players.Add(new Core.Common.Data.Player.Player { Email = email, BrandId = brand139.Id });

            var testCase = new TestPair(r => r.Email, email);
            ValidateTestCase(testCase, true);

            FakeBrandRepository.Brands.Add(brand139);

            ValidateTestCase(testCase);
        }

        [Test]
        public void Player_regisration_fails_when_passwords_do_not_match()
        {
            ValidateTestCase(new TestPair(r => r.PasswordConfirm, TestDataGenerator.GetRandomString()));
        }

        private void ValidateTestCase(TestPair propertyValuePair, bool validCase = false)
        {
            var propertyName = GetPropertyName(propertyValuePair.Item1);

            var registrationData = TestDataGenerator.CreateRandomRegistrationData();

            SetPropertyValue(registrationData, propertyName, propertyValuePair.Item2);
            if (propertyName == "Password")
            {
                SetPropertyValue(registrationData, "PasswordConfirm", propertyValuePair.Item2);
            }

            if (validCase)
            {
                var result = PlayerCommands.Register(registrationData);
                Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            }
            else
            {
                var e = Assert.Throws<ValidationException>(() => PlayerCommands.Register(registrationData));
                Assert.That(e.Errors.Count(), Is.GreaterThanOrEqualTo(1));
                Assert.That(e.Errors.First().ErrorMessage, Is.Not.Empty);
            }
        }

        #region Test helpers

        private static void SetPropertyValue(RegistrationData registrationData, string fieldName, object emptyValue)
        {
            var property = registrationData.GetType().GetProperty(fieldName);
            property.SetValue(registrationData, emptyValue);
        }

        private static string GetPropertyName(Expression<Func<RegistrationData, object>> propertyExpression)
        {
            return GetMemberInfo(propertyExpression).Member.Name;
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
            {
                throw new ArgumentException("method");
            }

            return memberExpr;
        }

        #endregion
    }

    [Serializable]
    public sealed class Pair<TKey, TValue> : Tuple<TKey, TValue>
    {
        public Pair(TKey first, TValue second) : base(first, second) { }

        public TKey Key => Item1;

        public TValue Value => Item2;
    }
}
