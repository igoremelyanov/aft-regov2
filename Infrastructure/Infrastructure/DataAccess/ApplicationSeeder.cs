using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.UGS.Core.BaseModels.Enums;

namespace AFT.RegoV2.Infrastructure.DataAccess
{
    public partial class ApplicationSeeder
    {
        private readonly IRepositoryBase _repositoryBase;
        private readonly IBrandRepository _brandRepository;
        private readonly ILicenseeCommands _licenseeCommands;
        private readonly ICurrencyCommands _currencyCommands;
        private readonly IBrandCommands _brandCommands;
        private readonly ICultureCommands _cultureCommands;
        private readonly IPlayerRepository _playerRepository;
        private readonly PlayerCommands _playerCommands;
        private readonly IAuthCommands _authCommands;
        private readonly IAuthRepository _authRepository;
        private readonly RoleService _roleService;
        private readonly ISecurityRepository _securityRepository;
        private readonly IBrandOperations _walletCommands;
        private readonly IBankCommands _bankCommands;
        private readonly IPaymentLevelCommands _paymentLevelCommands;
        private readonly IBankAccountCommands _bankAccountCommands;
        private readonly RiskLevelCommands _riskLevelCommands;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IGameManagement _gameManagement;
        private readonly ICurrencyExchangeCommands _currencyExchangeCommands;
        private readonly ContentTranslationCommands _contentTranslationCommands;
        private readonly BackendIpRegulationService _backendIpRegulationService;
        private readonly BrandIpRegulationService _brandIpRegulationService;
        private readonly IPaymentGatewaySettingsCommands _paymentGatewaySettingsCommands;
        private readonly ISettingsCommands _settingsCommands;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ICommonSettingsProvider _settingsProvider;
        private readonly IMassMessageCommands _massMessageCommands;
        private readonly IMessagingRepository _messagingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventBus _eventBus;

        public ApplicationSeeder(
            IRepositoryBase repositoryBase,
            IBrandRepository brandRepository,
            ILicenseeCommands licenseeCommands,
            ICurrencyCommands currencyCommands,
            IBrandCommands brandCommands,
            ICultureCommands cultureCommands,
            IPlayerRepository playerRepository,
            IBrandOperations walletCommands,
            PlayerCommands playerCommands,
            IAuthCommands authCommands,
            IAuthRepository authRepository,
            RoleService roleService,
            ISecurityRepository securityRepository,
            IBankCommands bankCommands,
            IBankAccountCommands bankAccountCommands,
            IPaymentLevelCommands paymentLevelCommands,
            RiskLevelCommands riskLevelCommands,
            IPaymentRepository paymentRepository,
            IGameRepository gameRepository,
            IGameManagement gameManagement,
            ICurrencyExchangeCommands currencyExchangeCommands,
            ContentTranslationCommands contentTranslationCommands,
            BackendIpRegulationService backendIpRegulationService,
            BrandIpRegulationService brandIpRegulationService,
            IPaymentGatewaySettingsCommands paymentGatewaySettingsCommands,
            ISettingsCommands settingsCommands,
            ISettingsRepository settingsRepository,
            ICommonSettingsProvider settingsProvider, 
            IMassMessageCommands massMessageCommands, 
            IMessagingRepository messagingRepository,
            IEventRepository eventRepository,
            IEventBus eventBus)
        {
            _repositoryBase = repositoryBase;
            _brandRepository = brandRepository;
            _licenseeCommands = licenseeCommands;
            _currencyCommands = currencyCommands;
            _brandCommands = brandCommands;
            _cultureCommands = cultureCommands;
            _playerCommands = playerCommands;
            _authCommands = authCommands;
            _authRepository = authRepository;
            _roleService = roleService;
            _securityRepository = securityRepository;
            _playerRepository = playerRepository;
            _walletCommands = walletCommands;
            _bankCommands = bankCommands;
            _bankAccountCommands = bankAccountCommands;
            _paymentLevelCommands = paymentLevelCommands;
            _riskLevelCommands = riskLevelCommands;
            _paymentRepository = paymentRepository;
            _gameRepository = gameRepository;
            _gameManagement = gameManagement;
            _currencyExchangeCommands = currencyExchangeCommands;
            _contentTranslationCommands = contentTranslationCommands;
            _backendIpRegulationService = backendIpRegulationService;
            _brandIpRegulationService = brandIpRegulationService;
            _paymentGatewaySettingsCommands = paymentGatewaySettingsCommands;
            _settingsCommands = settingsCommands;
            _settingsRepository = settingsRepository;
            _settingsProvider = settingsProvider;
            _massMessageCommands = massMessageCommands;
            _messagingRepository = messagingRepository;
            _eventRepository = eventRepository;
            _eventBus = eventBus;
        }

        public void InitializeSqlServerSessionStore()
        {
            var resourceName = "AFT.RegoV2.Infrastructure.DataAccess.SessionStorage.InitializeSQLServerSessionStore.sql";
            var databaseNamePattern = "__DATABASENAME__";
            
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var databaseName = builder.InitialCatalog;

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                string scriptSource = reader.ReadToEnd();

                // We need to have an actual database name in script
                var sqlQuery = scriptSource.Replace(databaseNamePattern, databaseName);

                // This separation by 'GO' is required because 'GO' is not an SQL statement but command for sql tools
                // to divide batches of operations. So we need to divide script by it manually and run all of the parts
                // one by one.
                var commandTexts = Regex.Split(sqlQuery, "^GO\\s*$", RegexOptions.Multiline);
                foreach (string commandText in commandTexts)
                {
                    _repositoryBase.ExecuteSqlCommand(commandText, true);
                }
            }
        }

        public void Seed()
        {
            AddSettings();

            PopulatePermissions();

            var licenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
            var brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
            var brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");

            var brands = new[] { brand138Id, brand831Id };
            var licenseeIds = new List<Guid> { licenseeId };

            var mockGameWebsite = _settingsProvider.GetGameWebsiteUrl();

            CreateSystemActor();
            CreateSuperAdmin(licenseeIds, brands);
            CreateFraudOfficerRole(licenseeIds);
            CreateKYCOfficerRole(licenseeIds);
            CreateCustomerServiceOfficerRole(licenseeIds);
            CreateMarketingOfficerRole(licenseeIds);
            CreatePaymentOfficer(licenseeIds);
            CreateDefaultRole(licenseeIds);
            CreateLicenseeRole(licenseeIds);
            CreateBrandManagerRole(licenseeIds);

            AddCurrencies(new[] { "CNY", "CAD", "UAH", "EUR", "GBP", "USD", "RUB", "ALL", "BDT", "ZAR" });

            AddCountry("US", "United States");
            AddCountry("CA", "Canada");
            AddCountry("GB", "Great Britain");
            AddCountry("CN", "China");

            AddCultureCode("en-US", "English US", "English");
            AddCultureCode("en-GB", "English UK", "English");
            AddCultureCode("zh-CN", "Chinese Simplified", "Chinese");
            AddCultureCode("zh-TW", "Chinese Traditional", "Chinese");

            AddSecurityQuestion(new Guid("96569808-4744-4bf2-952c-86b1a634bb67"), "What is your first pet?");
            AddSecurityQuestion(new Guid("46eb056d-72ae-4b89-bccb-f4ddf893c535"), "What was the name of the first street you lived on?");
            AddSecurityQuestion(new Guid("b355b02c-4de4-4981-9a09-5de74bfc5765"), "What is your mother's maiden name?");
            AddSecurityQuestion(new Guid("ff621262-172b-40b7-831b-56dffe66af0b"), "What is your sister in law's name?");
            AddSecurityQuestion(new Guid("3be83483-8345-44bf-a0d6-7bebd61daf8d"), "What is the colour of your first car?");
            var defaultSecurityQuestionId = AddSecurityQuestion(new Guid("a59635c7-523d-4c74-b456-483eeb458b6d"), "What was your childhood nickname?");

            var memberWebSiteUrl = _settingsProvider.GetMemberWebsiteUrl();
            AddLicensee(licenseeId, "Flycow", "Flycow Inc.", "flycow@flycow.com");
            AddBrand(brand138Id, licenseeId, "138", "138", "Pacific Standard Time", PlayerActivationMethod.Automatic, "138@138.com", "17786554773", memberWebSiteUrl);
            AddBrand(brand831Id, licenseeId, "831", "831", "Pacific Standard Time", PlayerActivationMethod.Email, "831@831.com", "17786554773", "http://www.831fake.com");
            
            AssignBrandCountries(brand138Id, new[] { "CA", "CN", "GB", "US" });
            AssignBrandCountries(brand831Id, new[] { "CA", "CN", "GB", "US" });
            AssignBrandCultures(brand138Id, new[] { "en-US", "zh-TW" }, "en-US");
            AssignBrandCultures(brand831Id, new[] { "en-US", "zh-TW" }, "en-US");
            AssignBrandCurrencies(brand138Id, new[] { "CAD", "RMB" }, "CAD", "CAD");
            AssignBrandCurrencies(brand831Id, new[] { "CAD", "RMB" }, "CAD", "CAD");

            AddCurrencyExchange(brand138Id, "CAD", 1.0m);
            AddCurrencyExchange(brand138Id, "RMB", 4.77m);
            AddCurrencyExchange(brand831Id, "CAD", 1m);

            AddBetLimitGroup("Bronze", 1);
            AddBetLimitGroup("Silver", 2);
            AddBetLimitGroup("Gold", 3);
            AddBetLimitGroup("Platinum", 4);
            AddBetLimitGroup("Diamond", 5);

            var bronzeVipLevel138Id = new Guid("541F60EF-AEE7-408B-9B39-90289D49F6AD");
            var silverVipLevel138Id = new Guid("0447e567-bdc6-4330-979c-5e0984bfb626");
            var goldenVipLevel138Id = new Guid("30e9988c-afed-49a0-be6b-ad60f7a50beb");
            var platinumVipLevel138Id = new Guid("CE4DD8F4-9593-43EA-8CD6-154417BD289A");
            var diamondVipLevel138Id = new Guid("6CA12F6E-1356-47A4-8D8F-CAD4A74E174F");
            AddVipLevel(bronzeVipLevel138Id, brand138Id, "B", "Bronze", "Level 1: Bronze", "#b3dc6c", 1, true);
            AddVipLevel(silverVipLevel138Id, brand138Id, "S", "Silver", "Level 2: Silver", "#cabdbf", 2, false);
            AddVipLevel(goldenVipLevel138Id, brand138Id, "G", "Gold", "Level 3: Gold", "#fad165", 3, false);
            AddVipLevel(platinumVipLevel138Id, brand138Id, "P", "Platinum", "Level 4: Platinum", "#fad165", 4, false);
            AddVipLevel(diamondVipLevel138Id, brand138Id, "D", "Diamond", "Level 5: Diamond", "#fad165", 5, false);
            var bronzeVipLevel831Id = new Guid("FEE9D950-7C4D-41C3-954E-75E1324A5D7B");
            var silverVipLevel831Id = new Guid("5ABBDF77-1EE9-43BC-92C4-A54CDD7E356B");
            var goldenVipLevel831Id = new Guid("0EE9DECD-12D7-466F-B820-9FF1EB889BC5");
            var platinumVipLevel831Id = new Guid("C537DCA6-9D21-449D-86E9-518A1C730A08");
            var diamondVipLevel831Id = new Guid("7BB9D54D-AD21-483E-89E9-082D4A81506E");
            AddVipLevel(bronzeVipLevel831Id, brand831Id, "B", "Bronze", "Level 1: Bronze", "#b3dc6c", 1, true);
            AddVipLevel(silverVipLevel831Id, brand831Id, "S", "Silver", "Level 2: Silver", "#cabdbf", 2, false);
            AddVipLevel(goldenVipLevel831Id, brand831Id, "G", "Gold", "Level 3: Gold", "#fad165", 3, false);
            AddVipLevel(platinumVipLevel831Id, brand831Id, "P", "Platinum", "Level 4: Platinum", "#fad165", 4, false);
            AddVipLevel(diamondVipLevel831Id, brand831Id, "D", "Diamond", "Level 5: Diamond", "#fad165", 5, false);

            AddContentTranslation("en-GB", "MainWallet-UK", "Main Wallet", "Main Balance");
            AddContentTranslation("zh-CN", "MainWallet-CN", "Main Wallet", "?????");

            var bankAccountTypeBannedId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000001"), "Banned");
            var bankAccountTypeHighRiskId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000002"), "High Risk");
            var bankAccountTypeVIPId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000003"), "VIP");
            var bankAccountTypeRoyaltyId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000004"), "Royalty");
            var bankAccountTypeAffiliateId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000005"), "Affiliate");
            var bankAccountTypeBannerId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000006"), "Banner");
            var bankAccountTypeStatementHRId = AddBankAccountType(new Guid("00000000-0000-0000-0000-000000000007"), "Statement HR");
            
            var cadAccount1Id = Guid.Parse("B6755CB9-8F9A-4EBA-87E0-1ED5493B7534");
            var rmbAccount1Id = Guid.Parse("13672261-70AC-46E3-9E62-9E2E3AB77663");
            var cadAccount2Id = Guid.Parse("D38241CF-9553-4219-8E34-9D0D16294F48");
            var rmbAccount2Id = Guid.Parse("A0F727DA-3570-4871-8743-D2345A491E59");

            var bank_se45Id = Guid.Parse("4f299e19-ecd0-4095-b61b-e6945374fd88");
            AddBank(bank_se45Id, "SE45", "Bank of Canada", "CA", brand138Id);
            AddBankAccount(cadAccount1Id, "BoC1", "John Doe", "0045058398257466",
                bankAccountTypeVIPId, bank_se45Id, "Main", "Vancouver", "CAD", "Jane Doe", "9093077070", "B845D", brand138Id, licenseeId);

            var bank_gb29Id = Guid.Parse("90410165-3a61-4926-96b4-70c80d102aa0");
            AddBank(bank_gb29Id, "GB29", "Canadian Western Bank", "CA", brand138Id);
            AddBankAccount(new Guid("973311D5-FBF9-46D2-B0B9-C56E5BBAFDFD"), "CWB1", "Chris Fowler", "00290000926819",
                bankAccountTypeAffiliateId, bank_gb29Id, "Main", "Edmonton", "CAD", "John Roe", "8772380092", "C845D", brand138Id, licenseeId);

            var bank_6016Id = Guid.Parse("1c4ec24d-1db8-4199-a6f2-2ec8d0d28473");
            AddBank(bank_6016Id, "6016", "California Federal Bank", "US", brand138Id);
            AddBankAccount(new Guid("E102CCFA-C44D-4EEF-9AA3-E27F327D02E6"), "CFB1", "Cornelio Bagaria", "00290000926819",
                bankAccountTypeRoyaltyId, bank_6016Id, "Main", "Province 2", "USD", "Jane Roe", "8004272000", "C845F", brand138Id, licenseeId);

            var bank_nwbkId = Guid.Parse("0a6f1617-66e1-4219-8a10-599daf6bbb85");
            AddBank(bank_nwbkId, "NWBK", "HSBC", "GB", brand138Id);
            AddBankAccount(new Guid("86FE5A6D-7053-4279-87D7-9946901DECD3"), "HSBC1", "Canary Wharf", "00240000470915268",
                bankAccountTypeStatementHRId, bank_nwbkId, "Main", "Province 1", "GBP", "Johnnie Doe", "8772380092", "H845B", brand138Id, licenseeId);

            var bank_70AcId = Guid.Parse("49ae636d-9077-495f-9c78-28f29eb277dc");
            AddBank(bank_70AcId, "70AC", "Hua Xia Bank", "CN", brand138Id);
            AddBankAccount(rmbAccount1Id, "HXB1", "Beijing", "003912940494",
                bankAccountTypeBannerId, bank_70AcId, "Main", "Beijing Municipality", "RMB", "Janie Doe", "8317587285", "X845B", brand138Id, licenseeId);
            AddBankAccount(rmbAccount2Id, "HXB2", "Beijing", "003912940495",
                bankAccountTypeBannerId, bank_70AcId, "Main", "Beijing Municipality", "RMB", "Johnas Doe", "8317587287", "X845B", brand831Id, licenseeId);

            var bank_56AbId = Guid.Parse("d0d46b78-1146-4872-8def-1edb5a7e8119");
            AddBank(bank_56AbId, "56AB", "Vancity", "CA", brand831Id);
            AddBankAccount(cadAccount2Id, "GH3E", "Vancouver", "005432876523",
                bankAccountTypeAffiliateId, bank_56AbId, "Main", "BC", "CAD", "Janie Doe", "8317587285", "X845B", brand831Id, licenseeId);

            var paymentGatewayId = AddPaymentGatewaySettings(brand138Id, "XPay0", "XPAY", 0);

            AddPaymentLevel(new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33"), brand138Id, "CAD", "CADLevel", "CADLevel", true, true, cadAccount1Id, true, paymentGatewayId);
            AddPaymentLevel(new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9"), brand138Id, "RMB", "RMBLevel", "RMBLevel", true, true, rmbAccount1Id, true, paymentGatewayId);
            AddPaymentLevel(new Guid("54A8B43D-B200-43A0-BCB4-4E2623BD5353"), brand831Id, "CAD", "CADVan", "CADVan", true, false, cadAccount2Id, true);
            AddPaymentLevel(new Guid("8B5E16AB-B00A-4E4E-9BF6-3934F0391260"), brand831Id, "RMB", "RMBVan", "RMBVan", true, false, rmbAccount2Id, true);
            
            AddRiskLevel(new Guid("5B6EA085-9661-4FA9-8391-54704040FE91"), brand138Id, "VIP", 1, false);
            AddRiskLevel(new Guid("5B6EA085-9661-4FA9-8391-54704040FE92"), brand831Id, "New Players", 2, true);
            AddRiskLevel(new Guid("5B6EA085-9661-4FA9-8391-54704040FE93"), brand138Id, "Test Account", 3, false);
            AddRiskLevel(new Guid("5B6EA085-9661-4FA9-8391-54704040FE94"), brand831Id, "Multiple Accounts", 4, true);
            AddRiskLevel(new Guid("5B6EA085-9661-4FA9-8391-54704040FE95"), brand138Id, "Stolen Accounts", 5, false);

            var defaultGameUrl = "gameName={GameName}&gameid={GameId}&gameProviderId={GameProviderId}";
            var defaultProviderUrl = mockGameWebsite + "Game/Index?";

            var providerMockGpId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F");
            AddGameProviderWithConfiguration(providerMockGpId, "Mock Casino", "MOCK_CASINO", true, new Guid("1E6001D6-722F-4774-B59C-05EDE2A74DB9"),
                "Regular", defaultProviderUrl, "Casino", "MOCK_CASINO_CLIENT_ID", "MOCK_CLIENT_SECRET");
            AddGameProviderCurrency(providerMockGpId, "RMB", "RMB");
            var gameSlotsId = AddGameProviderGame(new Guid("67277E31-800C-4793-B029-AE8231E4B0FA"), providerMockGpId, "Slots", "Slots_game", defaultGameUrl, "SL-MOCK");
            var gameRouletteId = AddGameProviderGame(new Guid("C17F4D3F-2F99-42A4-A766-4493EFF6DB9F"), providerMockGpId, "Roulette", "Roulette_game", defaultGameUrl, "RL-MOCK");
            var gameBlackjackId = AddGameProviderGame(new Guid("BD8BF6F5-BC5D-4BC6-BD8E-C48E45DD0977"), providerMockGpId, "Blackjack", "Blackjack_game", defaultGameUrl, "BJ-MOCK");
            var gamePokerId = AddGameProviderGame(new Guid("B641B4E9-CA08-4443-8FD3-8D1A43727C3E"), providerMockGpId, "Poker", "Poker_game", defaultGameUrl, "PK-MOCK");

            var providerMockSbId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060");
            AddGameProviderWithConfiguration(providerMockSbId, "Mock Sport Bets", "MOCK_SPORT_BETS", true, new Guid("9FC056A8-D516-4864-B86F-77C5764749A5"),
                "Regular", defaultProviderUrl, "Sports", "MOCK_SPORTS_CLIENT_ID", "MOCK_CLIENT_SECRET");
            AddGameProviderCurrency(providerMockSbId, "RMB", "RMB");
            var gameHockeyId = AddGameProviderGame(new Guid("C18CEBA7-77D8-4E5E-8E1F-F046B7F7544F"), providerMockSbId, "Hockey", "Hockey_game", defaultGameUrl, "HC-MOCK");
            var gameFootballId = AddGameProviderGame(new Guid("BDCD4277-4FF7-46EF-B8ED-F4192E51F03C"), providerMockSbId, "Football", "Football_game", defaultGameUrl, "FOOTBALL");

            var providerFlycowId = new Guid("2F7E5735-AD42-4945-9B72-A3954C2BE07F");
            AddGameProviderWithConfiguration(providerFlycowId, "FlyCow", "FC", true, new Guid("E7638A03-ADF1-4B9A-B573-B114EABD9347"),
                "Regular", defaultProviderUrl, "Flycow", "FLYCOW_CLIENT_ID", "FLYCOW_CLIENT_SECRET");
            AddGameProviderCurrency(providerFlycowId, "RMB", "RMB");
            var gameBaccaratId = AddGameProviderGame(new Guid("F56799C4-D328-4963-B068-50162FEB49A0"), providerFlycowId, "Baccarat", "Baccarat", defaultGameUrl, "BCT-FLASH");
            var gameCrittersId = AddGameProviderGame(new Guid("4AD0348B-5803-40CD-91F9-572009759DBA"), providerFlycowId, "Critters", "Critters", defaultGameUrl, "CTS-FLASH");
            var gameFortuneGodId = AddGameProviderGame(new Guid("6C2A8A18-17F6-49F8-BA15-15D6A466B3C2"), providerFlycowId, "Fortune God", "Fortune_God", defaultGameUrl, "FG-FLASH");
            var gameFruitasticId = AddGameProviderGame(new Guid("115C8286-BA92-423A-9955-F8D6B9AB97E8"), providerFlycowId, "Fruitastic", "Fruitastic", defaultGameUrl, "FTC-FLASH");
            var gameLostGardenId = AddGameProviderGame(new Guid("B15A3A72-F342-45F6-9AFC-97B7951764DA"), providerFlycowId, "Lost Garden", "Lost_Garden", defaultGameUrl, "LG-FLASH");
            var gamePirateQueenId = AddGameProviderGame(new Guid("EFEACA48-9279-4ECA-B589-01478DCA6CBB"), providerFlycowId, "Pirate Queen", "Pirate_Queen", defaultGameUrl, "PQ-FLASH");
            var gameScubaViewId = AddGameProviderGame(new Guid("AD2D4D16-9CF7-4F7B-8772-E7E6174A8FB6"), providerFlycowId, "Scuba View", "Scuba_View", defaultGameUrl, "SV-FLASH");

            var providerSunbetId = new Guid("602D2FDA-9C54-4EF2-9223-F287EAD4FCFB");
            AddGameProviderWithConfiguration(providerSunbetId, "Sunbet", "SB", true, new Guid("E516C1B0-0DD5-459B-ADDD-C777E7978F7B"),
                "Regular", defaultProviderUrl, "Sunbet", "SUNBET_CLIENT_ID", "SUNBET_CLIENT_SECRET");
            AddGameProviderCurrency(providerSunbetId, "RMB", "RMB");
            var gameSunbetLobbyId = AddGameProviderGame(new Guid("E02D2F95-4FD4-4927-BA17-02E5BB9D715F"), providerSunbetId, "Sunbet Lobby", "Sunbet_Lobby", defaultGameUrl, "sunbetlobby");

            var providerTgpId = new Guid("13B9378A-D78E-4EDA-88E6-4E0A525D0573");
            AddGameProviderWithConfiguration(providerTgpId, "TGP Games", "TGP", true, new Guid("A04BDAED-A469-45A8-AE5E-011085791F13"),
                "Regular", defaultProviderUrl, "TGP", "TGP_CLIENT_ID", "TGP_CLIENT_SECRET");
            AddGameProviderCurrency(providerTgpId, "RMB", "RMB");
            var gameChineseTreasuresId = AddGameProviderGame(new Guid("147244CF-93A5-45C6-9A65-9459D94C6574"), providerTgpId, "Chinese Treasures", "Chinese_Treasures", defaultGameUrl, "ChineseTreasures");
            var gameDragonsLuckId = AddGameProviderGame(new Guid("D4032801-58C0-4AE9-9B67-24923AA50F92"), providerTgpId, "Dragon's Luck", "Dragons_Luck", defaultGameUrl, "DragonsLuck");
            var gameEpicJourneyId = AddGameProviderGame(new Guid("3ADEB7D6-5459-4276-A515-B1FE07303AD2"), providerTgpId, "Epic Journey", "Epic_Journey", defaultGameUrl, "EpicJourney");
            var gameGodofWealthId = AddGameProviderGame(new Guid("6C255C8E-655A-4033-B8BA-47DBF216E059"), providerTgpId, "God of Wealth", "God_of_Wealth", defaultGameUrl, "GodofWealth");
            var gameGoldenLampsId = AddGameProviderGame(new Guid("333651B7-0C8D-41B3-8EC6-19958F6391E9"), providerTgpId, "Golden Lamps", "Golden_Lamps", defaultGameUrl, "GoldenLamps");
            var gameGoldenLotusId = AddGameProviderGame(new Guid("CFFD3360-DDA3-46F7-AC17-AC7EC9BDF9E3"), providerTgpId, "Golden Lotus", "Golden_Lotus", defaultGameUrl, "GoldenLotus");
            var gameJadeCharmsId = AddGameProviderGame(new Guid("BCE5F764-503F-4263-BBD0-89F7A021D844"), providerTgpId, "Jade Charms", "Jade_Charms", defaultGameUrl, "JadeCharms");
            var gameLuckyWizardId = AddGameProviderGame(new Guid("605AA5C9-6BA2-4B78-9349-DADFC3724A1B"), providerTgpId, "Lucky Wizard", "Lucky_Wizard", defaultGameUrl, "LuckyWizard");
            var gameRedPhoenixRisingId = AddGameProviderGame(new Guid("732DFDEF-5629-4A5E-88D6-A760E9D5103F"), providerTgpId, "Red Phoenix Rising", "Red_Phoenix_Rising", defaultGameUrl, "RedPhoenixRising");
            var gameWildFightId = AddGameProviderGame(new Guid("52F6769E-127B-43F4-ACF3-EF269423AC78"), providerTgpId, "Wild Fight", "Wild_Fight", defaultGameUrl, "WildFight");
            var gameWildSpartansId = AddGameProviderGame(new Guid("CFA75496-C4A0-4FAF-9E83-15D0FD984EF9"), providerTgpId, "Wild Spartans", "Wild_Spartans", defaultGameUrl, "WildSpartans");

            var providerGoldDeluxeId = new Guid("4CCB0717-353C-4050-9221-0667A177E224");
            AddGameProviderWithConfiguration(providerGoldDeluxeId, "GoldDeluxe", "GD", true, new Guid("D6434757-3EFB-4720-AB31-37D219BBB700"),
                "Regular", defaultProviderUrl, "GoldDeluxe", "GOLDDELUXE_CLIENT_ID", "GOLDDELUXE_CLIENT_SECRET");
            AddGameProviderCurrency(providerGoldDeluxeId, "RMB", "RMB");
            var gameGoldDeluxeLobbyId = AddGameProviderGame(new Guid("6AB53D68-9336-4DD7-A44E-D3067BE7549F"), providerGoldDeluxeId, "Gold Deluxe Lobby", "Gold_Deluxe_Lobby", defaultGameUrl, "golddeluxelobby");
            
            var lobbyMobile138 = new Guid("53D1F703-A016-480D-80BD-A3D4CAEB6530");
            AddLobby(lobbyMobile138, "138 mobile lobby", "Lobby-AftRego-Mobile", "XLaCbxQ8p2j8KwxUmZCLmKSKpxu2BqeSqArxbsjMYEm", PlatformType.Mobile);
            var lobbyDesktop138 = new Guid("E931A577-5E88-42FB-B5F0-91D3D32CBCE5");
            AddLobby(lobbyDesktop138, "138 desktop lobby", "Lobby-AftRego", "BVVsHKO6bvAFprykelW9hEaj8lHqS6mvd6aQ1KX7Luo", PlatformType.Desktop);
            var lobbyMobile831 = new Guid("7BD6784A-FE46-4333-B1F1-3153C11C9E07");
            AddLobby(lobbyMobile831, "831 mobile lobby", "Lobby-AftRego-Mobile-831", "XLaCbxQ8p2j8KwxUmZCLmKSKpxu2BqeSqArxbsjMYEm", PlatformType.Mobile);
            var lobbyDesktop831 = new Guid("6CD9B734-DF78-4B32-B18D-3E9752A04C10");
            AddLobby(lobbyDesktop831, "831 desktop lobby", "Lobby-AftRego-831", "BVVsHKO6bvAFprykelW9hEaj8lHqS6mvd6aQ1KX7Luo", PlatformType.Desktop);

            AssignLobbiesToBrand(brand138Id, new[] { lobbyMobile138, lobbyDesktop138 });
            AssignLobbiesToBrand(brand831Id, new[] { lobbyMobile831, lobbyDesktop831 });

            var ggMobileCasino138Id = new Guid("4CB7A91C-8DED-448D-92CA-FC8DFD35E458");
            AddGameGroupWithGames(ggMobileCasino138Id, lobbyMobile138, "Mock Casino Games", "Mock Casino Games", new[] { gameSlotsId , gameRouletteId, gameBlackjackId, gamePokerId});
            var ggMobileSports138Id = new Guid("F806701C-8061-437C-8B3A-52339F4240B6");
            AddGameGroupWithGames(ggMobileSports138Id, lobbyMobile138, "Mock Sport Bets Games", "Mock Sport Bets Games", new[] { gameHockeyId, gameFootballId });
            var ggDesktopCasino138Id = new Guid("5815B015-7B0D-432F-B988-3AEF409B7992");
            AddGameGroupWithGames(ggDesktopCasino138Id, lobbyDesktop138, "Mock Casino Games", "Mock Casino Games", new[] { gameSlotsId, gameRouletteId, gameBlackjackId, gamePokerId });
            var ggDesktopSports138Id = new Guid("62C531BC-93BC-4B7B-8CF3-E86C1C4A9E43");
            AddGameGroupWithGames(ggDesktopSports138Id, lobbyDesktop138, "Mock Sport Bets Games", "Mock Sport Bets Games", new[] { gameHockeyId, gameFootballId });

            var ggDesktopFeaturedGames138Id = new Guid("1090FC1E-26F8-4EB2-87DD-D779E6AB8F4E");
            AddGameGroupWithGames(ggDesktopFeaturedGames138Id, lobbyDesktop138, "Casino Featured Games", "FeaturedBanner",
                new[] { gameDragonsLuckId, gameFruitasticId, gameLostGardenId, gameBaccaratId, gameRedPhoenixRisingId, gameLuckyWizardId });
            var ggDesktopMainGames138Id = new Guid("2F66CB5A-7A74-43EE-935B-5D78C7486CF8");
            AddGameGroupWithGames(ggDesktopMainGames138Id, lobbyDesktop138, "Casino Main Games", "CategoryTab1",
                new[] 
                {
                    gameChineseTreasuresId, gameWildFightId, gameEpicJourneyId, gameFortuneGodId, gameCrittersId, gameScubaViewId,
                    gameWildSpartansId, gameGodofWealthId, gameGoldenLampsId, gameJadeCharmsId, gameGoldenLotusId, gamePirateQueenId,
                });
            var ggDesktopLiveDealer138Id = new Guid("45E89EC2-B62C-4656-97C2-4F30C1F57EFB");
            AddGameGroupWithGames(ggDesktopLiveDealer138Id, lobbyDesktop138, "Live Dealer", "CategoryTab2",
                new[] { gameSunbetLobbyId, gameGoldDeluxeLobbyId });
            var ggOverviewFeature138Id = new Guid("C69B69DE-DC51-4C33-AAFD-53D8482B9A59");
            AddGameGroupWithGames(ggOverviewFeature138Id, lobbyDesktop138, "Overview Feature Games", "CategoryTab3",
                new[] 
                {
                    gameSunbetLobbyId, gameGoldDeluxeLobbyId, gameFortuneGodId, gameDragonsLuckId, gameFruitasticId,
                    gameGoldenLampsId, gameWildFightId, gameRedPhoenixRisingId, gameBaccaratId,
                });
            var ggOverviewCasino138Id = new Guid("F57F0E56-0F66-4157-BE9B-88378D081B92");
            AddGameGroupWithGames(ggOverviewCasino138Id, lobbyDesktop138, "Overview Casino", "CategoryTab4",
                new[] 
                {
                    gameFruitasticId, gameBaccaratId, gameFortuneGodId, gameWildFightId, gameGodofWealthId,
                    gameJadeCharmsId, gameLostGardenId, gameCrittersId, gameChineseTreasuresId
                });
            var ggRecommended138Id = new Guid("6B6BF1F7-8E95-447C-92F4-13958513B415");
            AddGameGroupWithGames(ggRecommended138Id, lobbyDesktop138, "Recommended Games", "CategoryTab5",
                new[] { gameLuckyWizardId, gameWildSpartansId, gameLostGardenId });

            var ggDesktopCasino831Id = new Guid("B8A46250-4230-41AB-A60B-25B44A804431");
            AddGameGroupWithGames(ggDesktopCasino831Id, lobbyDesktop831, "Mock Casino Games", "Featured Games", new[] { gameSlotsId, gameRouletteId, gameBlackjackId, gamePokerId });
            var ggDesktopSports831Id = new Guid("32BA06F1-40EF-403A-B529-B1C1C6C188B7");
            AddGameGroupWithGames(ggDesktopSports831Id, lobbyDesktop831, "Mock Sport Bets Games", "Main Games", new[] { gameHockeyId, gameFootballId });

            CreateBrandsWalletStructure(licenseeId, brand138Id, brand831Id, providerMockGpId, providerMockSbId);

            AssignLicenseeProducts(licenseeId, providerMockGpId, providerMockSbId, providerFlycowId, providerGoldDeluxeId, providerSunbetId, providerTgpId);
            AssignBrandProducts(brand138Id, providerMockGpId, providerMockSbId, providerFlycowId, providerGoldDeluxeId, providerSunbetId, providerTgpId);
            AssignBrandProducts(brand831Id, providerMockGpId, providerMockSbId, providerFlycowId, providerGoldDeluxeId, providerSunbetId, providerTgpId);            
            
            AddBetLimits(brand138Id, providerMockGpId);

            AssignBrandCredentials(brand138Id, "AFTRego", "BsULoUoc1dOFBFouDHgWXpyU8kRHBeIUzT0MEmJt3fgi");
            AssignBrandCredentials(brand831Id, "AFTRego831", "BsULoUoc1dOFBFouDHgWXpyU8kRHBeIUzT0MEmJt3fgi");

            ActivateBrand(brand138Id);
            ActivateBrand(brand831Id);

            AddPlayer(brand138Id, "Test", "Player", "testplayer", defaultSecurityQuestionId, "123456", "CAD", "en-US", 0, null, true, false);
            AddPlayer(brand138Id, "Test", "User", "testuser", defaultSecurityQuestionId, "123456", "CAD", "en-US", 0, null, true, false);
            AddPlayer(brand138Id, "Locked", "User", "lockeduser", defaultSecurityQuestionId, "123456", "CAD", "en-US", 0, null, true, true);
            AddPlayer(brand138Id, "Inactive", "User", "inactiveuser", defaultSecurityQuestionId, "123456", "CAD", "en-US", 0, null, false, false);
            AddPlayer(brand138Id, "Demo", "Player", "demoplayer", defaultSecurityQuestionId, "123456", "RMB", "zh-TW", 0, null, true, false);

            AddBrandIpRegulationsBrand(brand138Id, licenseeId, "Pacific Standard Time");

            string ipAddress = "192.168.1.1";
            SetIpVerificationDisabled();
            AddAdminIpRegulations(ipAddress);
            AddBrandIpRegulations(licenseeId, brand138Id, ipAddress, "http://test.com");

            SendMassMessages(brand138Id);
            SendRegoHeadSeededEvent();
        }

        private void AddCultureCode(string code, string name, string nativeName)
        {
            if (_brandRepository.Cultures.Any(c => c.Code == code))
                return;

            _cultureCommands.Save(new EditCultureData { Code = code, Name = name, NativeName = nativeName });
            _brandCommands.ActivateCulture(code, "Activated when database has been seeded on first application start");
        }

        private Guid AddSecurityQuestion(Guid id, string question)
        {
            if (_playerRepository.SecurityQuestions.Any(c => c.Id == id))
                return id;

            return _playerCommands.CreateSecurityQuestion(new SecurityQuestion { Id = id, Question = question });
        }

        private void AddCurrencies(string[] codes)
        {
            var regionInfoGroups = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    const int invariantClutureId = 127;
                    if (culture.LCID == invariantClutureId)
                        return null;
                    return new RegionInfo(culture.LCID);
                })
                .Where(ri => ri != null)
                .GroupBy(ri => ri.ISOCurrencySymbol)
                .Where(x => codes.Contains(x.Key))
                .ToArray();

            foreach (var group in regionInfoGroups)
            {
                var code = group.Key;
                if (_brandRepository.Currencies.Any(x => x.Code == code))
                    continue;

                _currencyCommands.Add(new EditCurrencyData
                {
                    //AFTREGO-3538 Change Chinese Currency to "RMB"
                    Code = code.Equals("CNY") ? "RMB" : code,
                    Name = group.First().CurrencyEnglishName,
                    Remarks = "Created automatically while seeding database at first start"
                });
                _brandRepository.SaveChanges();
            }
        }

        private void AddCountry(string code, string name)
        {
            if (_brandRepository.Countries.Any(c => c.Code == code))
                return;

            _brandCommands.CreateCountry(code, name);
        }

        private void AddPlayer(Guid brandId, string firstName, string lastName, string username, Guid securityQuestionId,
            string password = "123456", string currency = "CAD", string culture = "en-US", decimal initialDeposit = 0m, string referralId = null, 
            bool isActive = true, bool isLocked = false, string state = "State/Province")
        {
            if (_playerRepository.Players.Any(x => x.Username == username))
                return;

            var playerData = new RegistrationData
            {
                BrandId = brandId.ToString(),
                CurrencyCode = currency,
                CultureCode = culture,
                Username = username,
                Password = password,
                PasswordConfirm = password,
                IdStatus = IdStatus.Verified.ToString(),
                IsLocked = isLocked,
                Comments = "Created by ApplicationSeeder",
                Gender = "Male",
                Title = "Mr",
                FirstName = firstName,
                LastName = lastName,
                Email = CreateEmailAddress(),
                AccountAlertSms = true,
                AccountAlertEmail = true,
                PhoneNumber = "123456789",
                MailingAddressLine1 = "305-1250 Homer Street",
                MailingAddressCity = "Vancouver",
                MailingAddressPostalCode = "V6B1C6",
                MailingAddressStateProvince = state,
                PhysicalAddressLine1 = "305-1250 Homer Street",
                PhysicalAddressCity = "Vancouver",
                PhysicalAddressPostalCode = "V6B1C6",
                CountryCode = "US",
                DateOfBirth = "1990/01/01",
                ContactPreference = "Email",
                SecurityAnswer = "1",
                SecurityQuestionId = securityQuestionId.ToString(),
                ReferralId = referralId,
                IpAddress = "127.0.0.1"
            };

            var playerId = _playerCommands.Register(playerData);
            _playerCommands.SetStatus(playerId, isActive);
            
            if (initialDeposit > 0m)
            {
                MakeDeposit(username, initialDeposit);
            }
        }

        private string CreateEmailAddress()
        {
            return Guid.NewGuid().ToString().Substring(0, 8) + "@mailinator.com";
        }

        public void MakeDeposit(string username, decimal depositAmount)
        {
            var player = _playerRepository.Players.Single(p => p.Username == username);
            MakeDeposit(player.Id, player.CurrencyCode, depositAmount);
        }

        public void MakeDeposit(Guid playerId, string currencyCode, decimal depositAmount = 200, string bonusCode = null)
        {
            _walletCommands.FundInAsync(playerId, depositAmount, currencyCode, Guid.NewGuid().ToString());
        }

        public void AddContentTranslation(string languageCode, string name, string source, string translation)
        {
            if (_brandRepository.ContentTranslations.Any(x => x.Name == name && x.Source == source && x.Language == languageCode))
                return;

            _contentTranslationCommands.CreateContentTranslation(new AddContentTranslationData
            {
                Language = languageCode,
                ContentName  = name,
                ContentSource = source,
                Translation = translation
            });

            var addedTranslation = _brandRepository.ContentTranslations.First(x => x.Name == name && x.Source == source && x.Language == languageCode);

            _contentTranslationCommands.ActivateContentTranslation(addedTranslation.Id, "Activated when database has been seeded on first application start");
        }



        private void SetIpVerificationDisabled()
        {
            _backendIpRegulationService.SetIpVerificationDisabled(true);
        }

        private void AddAdminIpRegulations(string ipAddress)
        {
            var regulation = _securityRepository.AdminIpRegulations.FirstOrDefault(x => x.IpAddress == ipAddress);
            if (regulation != null)
                return;

            _backendIpRegulationService.CreateIpRegulation(new AddBackendIpRegulationData()
            {
                IpAddress = ipAddress,
                Description = "test",
            });
        }

        //Temporary solution for Closed Beta R1.0
        private void AddBrandIpRegulationsBrand(Guid brandId, Guid licenseeId, string timeZoneId)
        {
            
            if (_securityRepository.Brands.Any(b => b.Id == brandId))
                return;

            _securityRepository.Brands.Add(new Core.Security.Data.Brand
            {
                Id = brandId,
                LicenseeId = licenseeId,
                TimeZoneId = timeZoneId,
            });
        }

        private void AddBrandIpRegulations(Guid licenseeId, Guid brandId, string ipAddress, string redirectionUrl)
        {
            var regulation = _securityRepository.BrandIpRegulations.FirstOrDefault(x => x.IpAddress == ipAddress && x.BrandId == brandId);
            if (regulation != null)
                return;

            _brandIpRegulationService.CreateIpRegulation(new AddBrandIpRegulationData()
            {
                LicenseeId = licenseeId,
                BrandId = brandId,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection, // Ip address is blocked with redirection to specified Url
                RedirectionUrl = redirectionUrl,
                IpAddress = ipAddress,
                Description = "test"
            });
        }

        private void SendMassMessages(Guid brandId)
        {
            if (_messagingRepository.MassMessages.Any())
                return;

            var oneTimeMessages = new Dictionary<string, string>()
            {
                {"Welcome to 138",@"You are now part of the {brand name} family, we are so happy to have you with us. 
To give you a smooth and enjoyable gaming experience we will keep you updated from time to time with our promotions and offers.
Make your way to the live dealer and casino games to get various bonuses. Place your first bet to get the fun started."},
                {"网站维护通知",@"敬请注意：网站将于2016年3月21日6:00-14:00进行升级维护，维护期间账户登录、网站游戏、存提款操作以及账户查询系统暂时无法运行，如您有任何疑问可以通过维护页面联系在线客服，或者拨打客服热线006323041688，以及发送邮件至客服邮箱 cs@sss988.com 进行咨询，不便之处敬请谅解！"},
                {"线下存款账号更新通知","温馨提示：尊敬的客户，网站的线下存款银行账户不定期更新，请您存款前先到网站充值申请页面获取最新的账户再进行存款，以便相关部门查询办理。不便之处敬请谅解！（在线支付不受影响）感谢您对网站的支持。"}
            };

            foreach (var message in oneTimeMessages)
            {
                SendMassMessage(brandId, message.Key, message.Value);
            }

            const int repeatedMessageCount = 18;
            const string repeatedMessageSubject = "Server Maintenance Announcement";
            const string repeatedMessageContent = @"To ensure the best gameplay experience, we will be running a maintenance for 30 minutes from 7:35pm ~ 8:00pm PDT. Minor issues are being fixed and we will be playable in no time.
Thank you very much for your patience.
Schedule: 10/15 7:35pm ~ 8:00pm PDT";

            for (var i = 0; i < repeatedMessageCount; i++)
            {
                SendMassMessage(brandId, repeatedMessageSubject, repeatedMessageContent);
            }
        }

        private void SendMassMessage(Guid brandId, string subject, string content)
        {
            var updateRecipientsResponse = _massMessageCommands.UpdateRecipients(new UpdateRecipientsRequest
            {
                UpdateRecipientsType = UpdateRecipientsType.SearchResultSelectAll,
                SearchPlayersRequest = new SearchPlayersRequest
                {
                    BrandId = brandId
                }
            });

            _massMessageCommands.Send(new SendMassMessageRequest
            {
                Id = updateRecipientsResponse.Id,
                Content = updateRecipientsResponse.Languages.Select(x => new SendMassMessageContent
                {
                    LanguageCode = x.Code,
                    OnSite = true,
                    OnSiteSubject = subject,
                    OnSiteContent = content
                }).ToArray()
            });
        }

        private void AddSettings()
        {
            var settingsToSeed = new Dictionary<string, string>();

            var seedSettingsPrefix = _settingsProvider.SeedSettingsPrefix;
            var keysToSeed = ConfigurationManager.AppSettings.AllKeys.Where(x => x.StartsWith(seedSettingsPrefix));

            foreach (var key in keysToSeed)
            {
                var settingsKey = key.Replace(seedSettingsPrefix, string.Empty);

                var existingSettingsItem = _settingsRepository.Settings.FirstOrDefault(x=>x.Key == settingsKey);
                if (existingSettingsItem != null)
                {
                    continue;
                }

                settingsToSeed.Add(settingsKey, ConfigurationManager.AppSettings[key]);
            }

            _settingsCommands.Save(settingsToSeed);
        }

        private void SendRegoHeadSeededEvent()
        {
            if(_eventRepository.GetEvents<RegoHeadSeeded>().Any() == false)
                _eventBus.Publish(new RegoHeadSeeded());
        }
    }
}