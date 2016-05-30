using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class GameSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<PlayerUpdated>,
        IConsumes<LicenseeCreated>,
        IConsumes<BrandProductsAssigned>,
        IConsumes<VipLevelRegistered>,
        IConsumes<VipLevelUpdated>,
        IConsumes<CurrencyCreated>,
        IConsumes<CurrencyUpdated>,
        IConsumes<LanguageCreated>,
        IConsumes<LanguageUpdated>,
        IConsumes<UgsGameEvent>
    {
        private readonly IGameRepository _repository;
        private readonly IUgsGameCommandsAdapter _gameCommandsAdapter;

        public GameSubscriber(IGameRepository repository, IUgsGameCommandsAdapter gameCommandsAdapter )
        {
            _repository = repository;
            _gameCommandsAdapter = gameCommandsAdapter;
        }

        public void Consume(PlayerRegistered @event)
        {
            var gamePlayer = new Player
            {
                Id = @event.PlayerId,
                Name = @event.UserName,
                DisplayName = @event.DisplayName,
                BrandId = @event.BrandId,
                CultureCode = @event.CultureCode,
                CurrencyCode = @event.CurrencyCode,
                VipLevelId = @event.VipLevelId
            };
            _repository.Players.Add(gamePlayer);
            _repository.SaveChanges();
        }

        public void Consume(LicenseeCreated @event)
        {
            _repository.Licensees.Add(new Licensee {Id = @event.Id});
            _repository.SaveChanges();
        }

        public void Consume(BrandRegistered @event)
        {
            var brand = new Interface.Data.Brand
            {
                Id = @event.Id,
                Code = @event.Code,
                LicenseeId = @event.LicenseeId,
                TimezoneId = @event.TimeZoneId
            };

            _repository.Brands.Add(brand);

            _repository.SaveChanges();
        }

        public void Consume(BrandUpdated @event)
        {
            var brand = _repository.Brands.Single(x => x.Id == @event.Id);

            brand.Code = @event.Code;
            brand.TimezoneId = @event.TimeZoneId;

            _repository.SaveChanges();
        }

        public void Consume(PlayerUpdated @event)
        {
            var player = _repository.Players.Single(x => x.Id == @event.Id);

            player.VipLevelId = @event.VipLevelId;

            _repository.SaveChanges();
        }

        public void Consume(BrandProductsAssigned @event)
        {
            var brand = _repository.Brands
                .Where(x => @event.BrandId == x.Id)
                .Include(x => x.BrandGameProviderConfigurations)
                .Single();

            var existingAssignees = new List<Guid>();

            if (brand.BrandGameProviderConfigurations != null)
                existingAssignees = brand.BrandGameProviderConfigurations
                    .Select(x => x.GameProviderId)
                    .ToList();

            brand.BrandGameProviderConfigurations = new Collection<BrandGameProviderConfiguration>();

            var products = @event.ProductsIds.Except(existingAssignees);

            var gameProviders = 
                from gp in _repository.GameProviders
                where products.Contains(gp.Id)
                select gp;

            if (gameProviders.Any() == false)
                return;

            var fullGps = gameProviders
                .Include(x => x.GameProviderConfigurations);

            foreach (var gameProvider in fullGps)
            {
                if ( gameProvider.GameProviderConfigurations == null || !gameProvider.GameProviderConfigurations.Any())
                    throw new RegoException("Unable to assign product. One of more products do not have configurations available.");

                var gameProviderConfiguration = gameProvider.GameProviderConfigurations.First();
                brand.BrandGameProviderConfigurations.Add(new BrandGameProviderConfiguration
                {
                    Id = Guid.NewGuid(),
                    BrandId = brand.Id,
                    GameProviderConfigurationId = gameProviderConfiguration.Id,
                    GameProviderId = gameProvider.Id
                });
            }

            _repository.SaveChanges();
        }

        public void Consume(VipLevelRegistered @event)
        {
            _repository.VipLevels.Add(new VipLevel 
            {
                Id = @event.Id,
                BrandId = @event.BrandId,
                VipLevelLimits = @event.VipLevelLimits.Select(x => new VipLevelGameProviderBetLimit
                {
                    VipLevelId = x.VipLevelId,
                    BetLimitId = x.BetLimitId,
                    GameProviderId = x.GameProviderId,
                    CurrencyCode = x.CurrencyCode
                }).ToList()
            });

            var betLimitGroup = _repository.BetLimitGroups.SingleOrDefault(blg => blg.Name == @event.Name);
            if (betLimitGroup != null)
            {
                _repository.VipLevelBetLimitGroups.AddOrUpdate(new VipLevelBetLimitGroup()
                {
                    VipLevelId = @event.Id,
                    BetLimitGroupId = betLimitGroup.Id
                });
            }

            _repository.SaveChanges();
        }

        public void Consume(VipLevelUpdated @event)
        {
            var vipLevel = _repository.VipLevels
                .Include(x => x.VipLevelLimits)
                .Single(x => x.Id == @event.Id);
            vipLevel.VipLevelLimits.Clear();
            vipLevel.VipLevelLimits = @event.VipLevelLimits.Select(x => new VipLevelGameProviderBetLimit
            {
                VipLevelId = x.VipLevelId,
                BetLimitId = x.BetLimitId,
                GameProviderId = x.GameProviderId,
                CurrencyCode = x.CurrencyCode
            }).ToList();

            var vipLevelBetLimitGroup = _repository.VipLevelBetLimitGroups.SingleOrDefault(blg => blg.VipLevelId == @event.Id);
            if (vipLevelBetLimitGroup != null)
            {
                _repository.VipLevelBetLimitGroups.Remove(vipLevelBetLimitGroup);
            }

            var betLimitGroup = _repository.BetLimitGroups.SingleOrDefault(blg => blg.Name == @event.Name);
            if(betLimitGroup != null)
            {
                _repository.VipLevelBetLimitGroups.Add(new VipLevelBetLimitGroup()
                {
                    VipLevelId = @event.Id,
                    BetLimitGroupId = betLimitGroup.Id
                });
            }

            _repository.SaveChanges();
        }

        public void Consume(LanguageCreated @event)

        {
            var culture = new GameCulture {Code = @event.Code};
            _repository.Cultures.Add(culture);
            _repository.SaveChanges();
        }

        public void Consume(LanguageUpdated @event)
        {
            var cultures = _repository.Cultures.Single(x => x.Code == @event.Code);
            cultures.Code = @event.Code;
            _repository.SaveChanges();
        }

        public void Consume(CurrencyCreated @event)
        {
            var currency = new GameCurrency { Code = @event.Code };
            _repository.Currencies.Add(currency);
            _repository.SaveChanges();
        }

        public void Consume(CurrencyUpdated @event)
        {
            var currencies = _repository.Currencies.Single(x => x.Code == @event.Code);
            currencies.Code = @event.Code;
            _repository.SaveChanges();
        }

        public void Consume(UgsGameEvent @event)
        {
            _gameCommandsAdapter.ConsumeGameEvent(@event);
        }
    }
}
