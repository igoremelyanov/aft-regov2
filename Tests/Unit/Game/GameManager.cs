using System;
using System.Linq;

using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Tests.Common;

namespace AFT.RegoV2.Tests.Unit.Game
{
    internal class GameManager : AdminWebsiteUnitTestsBase
    {
        private IGameManagement _gameManagement;
        private IGameQueries _queries;
        private IGameRepository _repository;

        public override void BeforeEach()
        {
            base.BeforeEach();
            
             _queries = Container.Resolve<IGameQueries>();
             _gameManagement = Container.Resolve<IGameManagement>();
            _repository = Container.Resolve<FakeGameRepository>();

            _repository.GameProviders.Add(
                new GameProvider
                {
                    Id = Guid.NewGuid(),
                    Name = "Mock Sport Bets",
                    Code = "Mock_Sport_Bets_code"
                });

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();
        }

        [Test]
        public void Can_Create_Game()
        {
            Assert.AreEqual(0, _queries.GetGameDtos().Count());

            var gameDto = new GameDTO
            {
                Name = "Game",
                Code = "Code",
                Type = "Casino",
                Status = "Active",
                Url = "http://localhost/",
                ProductId = _repository.GameProviders.First().Id
            };

            _gameManagement.CreateGame(gameDto);

            Assert.AreEqual(1, _queries.GetGameDtos().Count());
        }


        [Test]
        public void Can_Edit_Game()
        {
            Assert.AreEqual(0, _queries.GetGameDtos().Count());

            var gameDto = new GameDTO
            {
                Name = "Game",
                Code = "Code",
                Type = "Casino",
                Status = "Active",
                Url = "http://localhost/",
                ProductId = _repository.GameProviders.First().Id
            };

            _gameManagement.CreateGame(gameDto);

            Assert.AreEqual(1, _queries.GetGameDtos().Count());

            var game = _queries.GetGameDtos().First();

            game.Name = "Game updated";
            game.Code = "Code updated";

            _gameManagement.UpdateGame(game);

            game = _queries.GetGameDtos().First();

            Assert.AreEqual("Game updated", game.Name);
            Assert.AreEqual("Code updated", game.Code);
            Assert.NotNull(game.UpdatedBy);
            Assert.NotNull(game.UpdatedDate);
        }

        [Test]
        public void Can_Delete_Game()
        {
            Assert.AreEqual(0, _queries.GetGameDtos().Count());

            var gameDto = new GameDTO
            {
                Name = "Game",
                Code = "Code",
                Type = "Casino",
                Status = "Active",
                Url = "http://localhost/",
                ProductId = _repository.GameProviders.First().Id
            };

            _gameManagement.CreateGame(gameDto);

            Assert.AreEqual(1, _queries.GetGameDtos().Count());

            var game = _queries.GetGameDtos().First();

            _gameManagement.DeleteGame(game.Id.Value);

            Assert.AreEqual(0, _queries.GetGameDtos().Count());
        }
        
        [Test]
        public void Can_Create_GameProvider()
        {
            var gameProvider = new GameProvider
            {
                Id = Guid.NewGuid(),
                Name = "GameProvider",
                Code = "GameProviderCode",
            };

            Assert.AreEqual(0, _queries.GetGameProviders().Count(x => x.Id == gameProvider.Id));

            _gameManagement.CreateGameProvider(gameProvider);

            Assert.AreEqual(1, _queries.GetGameProviders().Count(x => x.Id == gameProvider.Id));
            Assert.NotNull(gameProvider.CreatedBy);
            Assert.NotNull(gameProvider.CreatedDate);
        }


        [Test]
        public void Can_Edit_GameProvider()
        {
            var gameProvider = new GameProvider
            {
                Id = Guid.NewGuid(),
                Name = "GameProvider",
                Code = "GameProviderCode",
            };

            Assert.AreEqual(0, _queries.GetGameProviders().Count(x => x.Id == gameProvider.Id));

            _gameManagement.CreateGameProvider(gameProvider);

            Assert.AreEqual(1, _queries.GetGameProviders().Count(x => x.Id == gameProvider.Id));

            gameProvider = _queries.GetGameProviders().First(x => x.Id == gameProvider.Id);

            gameProvider.Name = "gameProvider updated";
            gameProvider.Code = "gameProvider updated";

            _gameManagement.UpdateGameProvider(gameProvider);

            gameProvider = _queries.GetGameProviders().First(x => x.Id == gameProvider.Id);

            Assert.AreEqual("gameProvider updated", gameProvider.Name);
            Assert.AreEqual("gameProvider updated", gameProvider.Code);
            Assert.NotNull(gameProvider.UpdatedBy);
            Assert.NotNull(gameProvider.UpdatedDate);
        }

        [Test]
        public void Can_Create_BetLimitGroup()
        {
            var betLimitGroup = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };
            
            var betLimitGroupId = _gameManagement.CreateBetLimitGroup(betLimitGroup);
            var resultGroup = _queries.GetBetLimitGroup(betLimitGroupId);
            
            Assert.NotNull(resultGroup);
            Assert.AreEqual(betLimitGroup.Name, resultGroup.Name);
            Assert.AreEqual(betLimitGroup.ExternalId, resultGroup.ExternalId);
            Assert.NotNull(resultGroup.CreatedBy);
            Assert.NotNull(resultGroup.CreatedDate);
        }

        [Test]
        public void Can_Update_BetLimitGroup()
        {
            var betLimitGroup = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };
            
            var betLimitGroupId = _gameManagement.CreateBetLimitGroup(betLimitGroup);
            betLimitGroup = _queries.GetBetLimitGroup(betLimitGroupId);
            betLimitGroup.Name = TestDataGenerator.GetRandomAlphabeticString(7);
            betLimitGroup.ExternalId = TestDataGenerator.GetRandomNumber(50, 1);

            _gameManagement.UpdateBetLimitGroup(betLimitGroup);
            var resultGroup = _queries.GetBetLimitGroup(betLimitGroupId);

            Assert.NotNull(resultGroup);
            Assert.AreEqual(betLimitGroup.Name, resultGroup.Name);
            Assert.AreEqual(betLimitGroup.ExternalId, resultGroup.ExternalId);
            Assert.NotNull(resultGroup.UpdatedBy);
            Assert.NotNull(resultGroup.UpdatedDate);
            Assert.NotNull(resultGroup.CreatedBy);
            Assert.NotNull(resultGroup.CreatedDate);
        }

        [Test]
        public void VipLevel_Creation_Creates_Connection_To_BetLimitGroup_If_Name_Equals()
        {
            var betLimitGroup = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };
            
            var betLimitGroupId = _gameManagement.CreateBetLimitGroup(betLimitGroup);
            betLimitGroup = _queries.GetBetLimitGroup(betLimitGroupId);

            var vipLevelId = CreateVipLevel(betLimitGroup.Name);
            var betLimitGroupByVipLevel = _queries.GetBetLimitGroupByVipLevel(vipLevelId);

            Assert.NotNull(betLimitGroupByVipLevel);
            Assert.AreEqual(betLimitGroup.Id, betLimitGroupByVipLevel.Id);
        }

        [Test]
        public void VipLevel_Creation_Do_Not_Creates_Connection_To_BetLimitGroup_If_Name_Equals()
        {
            var betLimitGroup = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };
            
            var betLimitGroupId = _gameManagement.CreateBetLimitGroup(betLimitGroup);
            betLimitGroup = _queries.GetBetLimitGroup(betLimitGroupId);

            var vipLevelId = CreateVipLevel(betLimitGroup.Name + "x");
            var betLimitGroupByVipLevel = _queries.GetBetLimitGroupByVipLevel(vipLevelId);

            Assert.Null(betLimitGroupByVipLevel);
        }

        [Test]
        public void VipLevel_Update_Creates_Connection_To_BetLimitGroup_If_Name_Equals()
        {
            var betLimitGroup = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };

            var betLimitGroupId = _gameManagement.CreateBetLimitGroup(betLimitGroup);
            betLimitGroup = _queries.GetBetLimitGroup(betLimitGroupId);

            var vipLevelId = CreateVipLevel(betLimitGroup.Name + "x");
            var betLimitGroupByVipLevelAfterCreate = _queries.GetBetLimitGroupByVipLevel(vipLevelId);
            UpdateVipLevel(vipLevelId, betLimitGroup.Name);
            var betLimitGroupByVipLevelAfterUpdate = _queries.GetBetLimitGroupByVipLevel(vipLevelId);

            Assert.Null(betLimitGroupByVipLevelAfterCreate);
            Assert.NotNull(betLimitGroupByVipLevelAfterUpdate);
            Assert.AreEqual(betLimitGroup.Id, betLimitGroupByVipLevelAfterUpdate.Id);
        }

        [Test]
        public void VipLevel_Update_Remove_Connection_To_BetLimitGroup_If_Name_Not_Equals()
        {
            var betLimitGroup = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };

            var betLimitGroupId = _gameManagement.CreateBetLimitGroup(betLimitGroup);
            betLimitGroup = _queries.GetBetLimitGroup(betLimitGroupId);

            var vipLevelId = CreateVipLevel(betLimitGroup.Name);
            var betLimitGroupByVipLevelAfterCreate = _queries.GetBetLimitGroupByVipLevel(vipLevelId);
            UpdateVipLevel(vipLevelId, betLimitGroup.Name + "x");
            var betLimitGroupByVipLevelAfterUpdate = _queries.GetBetLimitGroupByVipLevel(vipLevelId);

            Assert.NotNull(betLimitGroupByVipLevelAfterCreate);
            Assert.Null(betLimitGroupByVipLevelAfterUpdate);
        }

        [Test]
        public void VipLevel_Update_Switches_Connection_To_BetLimitGroup_If_Name_Equals_Another_Group()
        {
            var betLimitGroup1 = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };

            var betLimitGroup2 = new BetLimitGroup
            {
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                ExternalId = TestDataGenerator.GetRandomNumber(50, 1),
            };

            var betLimitGroup1Id = _gameManagement.CreateBetLimitGroup(betLimitGroup1);
            var betLimitGroup2Id = _gameManagement.CreateBetLimitGroup(betLimitGroup2);
            betLimitGroup1 = _queries.GetBetLimitGroup(betLimitGroup1Id);
            betLimitGroup2 = _queries.GetBetLimitGroup(betLimitGroup2Id);

            var vipLevelId = CreateVipLevel(betLimitGroup1.Name);
            var betLimitGroupByVipLevelAfterCreate = _queries.GetBetLimitGroupByVipLevel(vipLevelId);
            UpdateVipLevel(vipLevelId, betLimitGroup2.Name);
            var betLimitGroupByVipLevelAfterUpdate = _queries.GetBetLimitGroupByVipLevel(vipLevelId);

            Assert.NotNull(betLimitGroupByVipLevelAfterCreate);
            Assert.NotNull(betLimitGroupByVipLevelAfterUpdate);
            Assert.AreEqual(betLimitGroup1.Id, betLimitGroupByVipLevelAfterCreate.Id);
            Assert.AreEqual(betLimitGroup2.Id, betLimitGroupByVipLevelAfterUpdate.Id);
        }

        private Guid CreateVipLevel(string name)
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var brandRepository = Container.Resolve<IBrandRepository>();
            var brandCommands = Container.Resolve<IBrandCommands>();

            var brand = brandTestHelper.CreateBrand();

            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(1000);
            } while (brandRepository.VipLevels.Any(vl => vl.Rank == rank));

            var vipLevel = new VipLevelViewModel
            {
                Name = name,
                Code = TestDataGenerator.GetRandomString(10),
                Brand = brand.Id,
                Rank = 1,
                IsDefault = false
            };

            return brandCommands.AddVipLevel(vipLevel);
        }

        private void UpdateVipLevel(Guid vipLevelId, string name)
        {
            var brandCommands = Container.Resolve<IBrandCommands>();
            var brandQueries = Container.Resolve<BrandQueries>();

            var vipLevel = brandQueries.GetVipLevel(vipLevelId);
            brandCommands.DeactivateVipLevel(vipLevel.Id, "deactivate for edit", null);

            var vipLevelModel = new VipLevelViewModel()
            {
                Id = vipLevel.Id,
                Name = name,
                Brand = vipLevel.BrandId,
                Code = vipLevel.Code,
                Rank = vipLevel.Rank,
                Description = vipLevel.Description,
                Color = vipLevel.ColorCode,
                Remark = "updated",
                IsDefault = false,
            };

            brandCommands.EditVipLevel(vipLevelModel);
            brandCommands.ActivateVipLevel(vipLevel.Id, "activation after edit");
        }
    }
}