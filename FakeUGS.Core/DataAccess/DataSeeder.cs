using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Configuration;

using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AFT.UGS.Core.BaseModels.Enums;

using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.DataAccess
{
    public class DataSeeder
    {
        private readonly IRepository _repository;
        private readonly IWalletOperations _walletCommands;
        private readonly IGameManagement _gameManagement;

        public DataSeeder(IRepository repository, IWalletOperations walletCommands, IGameManagement gameManagement)
        {
            _repository = repository;
            _walletCommands = walletCommands;
            _gameManagement = gameManagement;
        }

        public void Seed()
        {
            var fakeGameWebsite = ConfigurationManager.AppSettings["GameWebsiteUrl"];

            var licenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
            var brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
            var brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");

            AddLicensee(licenseeId);

            AddBrand(brand138Id, "138", licenseeId);
            AddBrand(brand831Id, "831", licenseeId);

            AddCurrencies(new[] { "RMB", "CAD", "UAH", "EUR", "GBP", "USD", "RUB", "ALL", "BDT", "ZAR" });


            AddCultureCode("en-US", "English US", "English");
            AddCultureCode("en-GB", "English UK", "English");
            AddCultureCode("zh-CN", "Chinese Simplified", "Chinese");
            AddCultureCode("zh-TW", "Chinese Traditional", "Chinese");

            var defaultGameUrl = "gameName={GameName}&gameid={GameId}&gameProviderId={GameProviderId}";
            var defaultProviderUrl = fakeGameWebsite + "Game/Index?";
            
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
            AddGameGroupWithGames(ggMobileCasino138Id, lobbyMobile138, "Mock Casino Games", "Mock Casino Games", new[] { gameSlotsId, gameRouletteId, gameBlackjackId, gamePokerId });
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

            AddBetLimits(brand138Id, providerMockGpId);

            AssignBrandProducts(brand138Id, providerMockGpId, providerMockSbId, providerFlycowId, providerGoldDeluxeId, providerSunbetId, providerTgpId);
            AssignBrandProducts(brand831Id, providerMockGpId, providerMockSbId, providerFlycowId, providerGoldDeluxeId, providerSunbetId, providerTgpId);

            AssignBrandCredentials(brand138Id, "AFTRego", "BsULoUoc1dOFBFouDHgWXpyU8kRHBeIUzT0MEmJt3fgi");
            AssignBrandCredentials(brand831Id, "AFTRego831", "BsULoUoc1dOFBFouDHgWXpyU8kRHBeIUzT0MEmJt3fgi");
        }

        private void AssignBrandProducts(Guid brandId, params Guid[] productIds)
        {
            var brand = _repository.Brands.Include(x => x.BrandGameProviderConfigurations).SingleOrDefault(x => x.Id == brandId);
            if (brand == null)
                return;

            foreach (var productId in productIds)
            {
                using (var scope = CustomTransactionScope.GetTransactionScope())
                {
                    var gameProvider = _repository.GameProviders.Include(x => x.GameProviderConfigurations).Single(x => x.Id == productId);
                    var gpcId = gameProvider.GameProviderConfigurations.First().Id;

                    if (brand.BrandGameProviderConfigurations.FirstOrDefault(
                        x => x.GameProviderId == gameProvider.Id && x.GameProviderConfigurationId == gpcId) != null)
                        continue;

                    brand.BrandGameProviderConfigurations.Add(new BrandGameProviderConfiguration
                    {
                        Id = Guid.NewGuid(),
                        BrandId = brand.Id,
                        GameProviderId = gameProvider.Id,
                        GameProviderConfigurationId = gpcId
                    });
                    _repository.SaveChanges();
                    scope.Complete();
                }
            }
        }

        public void AddLicensee(Guid id)
        {
            if (_repository.Licensees.Any(x => x.Id == id)) return;
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var licensee = new Licensee
                {
                    Id = id
                };
                _repository.Licensees.AddOrUpdate(licensee);

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        private void AddCurrencies(string[] codes)
        {
            foreach (var code in codes)
            {
                if (_repository.Currencies.Any(c => c.Code == code))
                    continue;
                using (var scope = CustomTransactionScope.GetTransactionScope())
                {
                    _repository.Currencies.AddOrUpdate(new GameCurrency
                    {
                        Code = code
                    });
                    _repository.SaveChanges();
                    scope.Complete();
                }
            }
        }

        private void AddCultureCode(string code, string name, string nativeName)
        {
            if (_repository.Cultures.Any(c => c.Code == code))
                return;
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Cultures.AddOrUpdate(new GameCulture
                {
                    Code =  code
                });
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void AddBrand(Guid id, string name, Guid licenseeId)
        {

            if (_repository.Brands.Any(x => x.Id == id)) return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brand = new Brand
                {
                    Id = id,
                    LicenseeId = licenseeId,
                    Name = name,
                    Code = name,
                    TimezoneId = "Pacific Standard Time"
                };
                _repository.Brands.AddOrUpdate(brand);

                _repository.SaveChanges();
                scope.Complete();
            }
        }


        public void AddGameProviderWithConfiguration(Guid providerId, string name, string code, bool isActive, Guid configurationId, string configurationName,
    string endpoint, string configurationType, string clientId, string clientSecret)
        {
            if (_repository.GameProviders.Any(x => x.Id == providerId))
                return;

            var gameProvider = new GameProvider
            {
                Id = providerId,
                Name = name,
                Code = code
            };

            var configuration = new GameProviderConfiguration
            {
                Id = configurationId,
                GameProviderId = providerId,
                Name = configurationName,
                AuthorizationClientId = clientId,
                AuthorizationSecret = clientSecret,
                SecurityKey = clientId + "_SECURITY_KEY",
                Endpoint = endpoint,
                Type = configurationType,
                IsActive = true
            };

            gameProvider.GameProviderConfigurations = new Collection<GameProviderConfiguration>
            {
                configuration
            };

            _gameManagement.CreateGameProvider(gameProvider);

            if (isActive)
            {
                gameProvider.IsActive = true;
                _gameManagement.UpdateGameProvider(gameProvider);
            }
        }

        public void AddGameProviderCurrency(Guid gameProviderId, string currencyCode, string gameProviderCurrencyCode)
        {
            if (_repository.GameProviderCurrencies.Any(x => x.GameProviderId == gameProviderId &&
                    x.CurrencyCode == currencyCode &&
                    x.GameProviderCurrencyCode == gameProviderCurrencyCode))
                return;

            var gameProviderCurrency = new GameProviderCurrency
            {
                GameProviderId = gameProviderId,
                CurrencyCode = currencyCode,
                GameProviderCurrencyCode = gameProviderCurrencyCode,
            };

            _gameManagement.CreateGameProviderCurrency(gameProviderCurrency);
        }
        
        private Guid AddGameProviderGame(Guid gameId, Guid gameProviderId, string name, string code, string endpointPath, string externalGameId)
        {
            if (_repository.Games.Any(x => x.Id == gameId))
                return gameId;

            _gameManagement.CreateGame(new GameDTO
            {
                Id = gameId,
                ProductId = gameProviderId,
                ExternalId = externalGameId,
                Name = name,
                Code = code,
                Url = endpointPath,
                Status = "Active",
            });

            return gameId;
        }

        private void AddBetLimits(Guid brand138Id, Guid providerMockGpId)
        {
            var brand = _repository.Brands.FirstOrDefault(x => x.Id == brand138Id);
            if (brand == null)
            {
                return;
            }

            var betLimitsCount = _repository.BetLimits.Count(x => x.BrandId == brand.Id && x.GameProviderId == providerMockGpId);
            if (betLimitsCount == 0)
            {
                _gameManagement.UpdateProductSettings(new BrandProductSettingsData
                {
                    BrandId = brand138Id,
                    ProductId = providerMockGpId,
                    BetLevels = new[]
                    {
                        new BetLevelData {Code = "10", Name = "BetLevel1"},
                        new BetLevelData {Code = "20", Name = "BetLevel2"},
                        new BetLevelData {Code = "30", Name = "BetLevel3"},
                    }
                });
            }
        }

        private void AddLobby(Guid id, string name, string clientId, string secret, PlatformType platformType, bool isActive = true)
        {
            var lobby = _repository.Lobbies.FirstOrDefault(x => x.Id == id);
            if (lobby != null)
                return;

            _gameManagement.CreateLobby(new Lobby
            {
                Id = id,
                Name = name,
                ClientId = clientId,
                ClientSecret = secret,
                PlatformType = platformType,
                IsActive = true
            });
        }

        private void AssignLobbiesToBrand(Guid brandId, Guid[] lobbiesIds)
        {
            var brand = _repository.Brands.Include(x => x.BrandLobbies).FirstOrDefault(x => x.Id == brandId);
            if (brand == null)
                return;

            if (brand.BrandLobbies.Select(x => x.LobbyId).ToArray().ScrambledEquals(lobbiesIds))
            {
                return;
            }

            _gameManagement.AssignLobbiesToBrand(new AssignLobbiesToBrandData
            {
                BrandId = brandId,
                LobbiesIds = lobbiesIds
            });
        }

        private void AddGameGroupWithGames(Guid gameGroupId, Guid lobbyId, string name, string code, Guid[] gamesIds)
        {
            var gameGroup = _repository.GameGroups.FirstOrDefault(x => x.Id == gameGroupId);
            if (gameGroup != null)
                return;

            _gameManagement.CreateGameGroup(new GameGroup
            {
                Id = gameGroupId,
                Name = name,
                Code = code,
                LobbyId = lobbyId,
            });

            _gameManagement.AssignGamesToGameGroups(new AssignGamesToGameGroupData
            {
                GameGroupId = gameGroupId,
                GamesIds = gamesIds
            });
        }

        public void AssignBrandCredentials(Guid brandId, string clientId, string clientSecret)
        {
            var brand = _repository.Brands.Single(x => x.Id == brandId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.ClientId = clientId;
                brand.ClientSecret = clientSecret;

                _repository.SaveChanges();
                scope.Complete();
            }
        }
    }
}