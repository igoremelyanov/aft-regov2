using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class WalletSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>,
        IConsumes<FundedIn>,
        IConsumes<FundedOut>

    {
        private readonly IGameRepository _walletRepository;

        public WalletSubscriber(IGameRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public void Consume(PlayerRegistered @event)
        {
            var brand = _walletRepository.Brands.Single(b => b.Id == @event.BrandId);

            CreateWalletsForPlayer(@event.PlayerId, @event.CurrencyCode, brand);

            _walletRepository.SaveChanges();
        }

        private void CreateWalletsForPlayer(Guid playerId, string currencyCode,  Core.Game.Interface.Data.Brand brand)
        {
            _walletRepository.Wallets.Add(new Wallet
            {
                PlayerId = playerId,
                Brand = brand,
                CurrencyCode = currencyCode
            });
        }

        public void Consume(FundedIn @event)
        {
            var wallet = _walletRepository.GetWalletWithUPDLock(@event.PlayerId);
            wallet.FundIn(@event.Amount, @event.TransactionId);
            _walletRepository.SaveChanges();
        }

        public void Consume(FundedOut @event)
        {
            var wallet = _walletRepository.GetWalletWithUPDLock(@event.PlayerId);
            wallet.FundOut(@event.Amount, @event.TransactionId);
            _walletRepository.SaveChanges();
        }
    }
}