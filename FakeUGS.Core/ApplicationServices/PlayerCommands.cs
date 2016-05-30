using System;
using System.Data.Entity.Migrations;

using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared.Utils;
using AFT.UGS.Core.Messages.Players;

using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ApplicationServices
{
    public sealed class PlayerCommands : IPlayerCommands
    {
        private readonly IRepository _repository;
        
        public PlayerCommands(IRepository repository)
        {
            _repository = repository;
        }

        public Player AddPlayerFromRequest(AuthorizePlayerRequest request, Guid brandId)
        {
            var player = new Player
            {
                Id = Guid.Parse(request.userid),
                BrandId = brandId,
                CurrencyCode = request.cur,
                CultureCode = request.lang,
                Name = request.username,
            };

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Players.AddOrUpdate(player);

                var wallet = new Wallet()
                {
                    Id = Guid.NewGuid(),
                    BrandId = player.BrandId,
                    Balance = 0,
                    CurrencyCode = player.CurrencyCode,
                    PlayerId = player.Id
                };
                _repository.Wallets.AddOrUpdate(wallet);

                _repository.SaveChanges();

                scope.Complete();
            }

            return player;
        }
    }
}