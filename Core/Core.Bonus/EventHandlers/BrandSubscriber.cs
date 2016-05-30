using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Shared;
using AutoMapper;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Bonus.Core.EventHandlers
{
    public class BrandSubscriber
    {
        private readonly IUnityContainer _container;
        private const string NoBrandFormat = "No brand found with Id: {0}";

        static BrandSubscriber()
        {
            Mapper.CreateMap<BrandRegistered, Brand>();
            Mapper.CreateMap<BrandUpdated, Brand>();
        }

        public BrandSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(BrandRegistered @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.Id);
            if (brand != null)
                throw new RegoException($"Brand already exist. Id: {@event.Id}");

            brand = Mapper.Map<BrandRegistered, Brand>(@event);
            bonusRepository.Brands.Add(brand);
            bonusRepository.SaveChanges();
        }

        public void Handle(BrandUpdated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.Id);
            if (brand == null)
                throw new RegoException(string.Format(NoBrandFormat, @event.Id));

            Mapper.Map(@event, brand);
            bonusRepository.SaveChanges();
        }

        public void Handle(BrandCurrenciesAssigned @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.BrandId);
            if (brand == null)
                throw new RegoException(string.Format(NoBrandFormat, @event.BrandId));

            brand.Currencies.Clear();
            foreach (var currency in @event.Currencies)
            {
                brand.Currencies.Add(new Currency { Code = currency });
            }
            bonusRepository.SaveChanges();
        }

        public void Handle(VipLevelRegistered @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.BrandId);
            if (brand == null)
                throw new RegoException(string.Format(NoBrandFormat, @event.BrandId));

            brand.Vips.Add(new VipLevel { Code = @event.Code });
            bonusRepository.SaveChanges();
        }

        public void Handle(WalletTemplateCreated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.BrandId);
            if (brand == null)
                throw new RegoException(string.Format(NoBrandFormat, @event.BrandId));

            foreach (var walletTemplate in @event.WalletTemplates)
            {
                brand.WalletTemplates.Add(new WalletTemplate
                {
                    Id = walletTemplate.Id,
                    IsMain = walletTemplate.IsMain,
                    Products = walletTemplate.ProductIds.Select(id => new Product { ProductId = id }).ToList()
                });
            }

            bonusRepository.SaveChanges();
        }

        public void Handle(WalletTemplateUpdated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.BrandId);
            if (brand == null)
                throw new RegoException(string.Format(NoBrandFormat, @event.BrandId));

            var brandPlayers = bonusRepository.Players.Where(p => p.Brand.Id == brand.Id);
            foreach (var walletTemplate in @event.NewWalletTemplates)
            {
                var walletTemplateData = new WalletTemplate
                {
                    Id = walletTemplate.Id,
                    IsMain = walletTemplate.IsMain,
                    Products = walletTemplate.ProductIds.Select(id => new Product {ProductId = id}).ToList()
                };
                brand.WalletTemplates.Add(walletTemplateData);

                foreach (var brandPlayer in brandPlayers)
                {
                    brandPlayer.Wallets.Add(new Wallet 
                    { 
                        Player = brandPlayer,
                        Template = walletTemplateData
                    });
                }
            }

            foreach (var walletTemplateDto in @event.RemainedWalletTemplates)
            {
                var walletTemplate = brand.WalletTemplates.Single(wt => wt.Id == walletTemplateDto.Id);

                walletTemplate.Products.Clear();
                walletTemplate.Products.AddRange(walletTemplateDto.ProductIds.Select(id => new Product { ProductId = id }));
                walletTemplate.IsMain = walletTemplate.IsMain;
            }

            bonusRepository.SaveChanges();
        }
    }
}