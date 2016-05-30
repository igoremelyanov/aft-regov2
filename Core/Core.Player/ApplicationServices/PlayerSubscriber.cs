using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class PlayerSubscriber : IBusSubscriber,
        IConsumes<VipLevelRegistered>,
        IConsumes<VipLevelUpdated>,
        IConsumes<BrandDefaultVipLevelChanged>,
        IConsumes<BetWon>,
        IConsumes<BetPlacedFree>,
        IConsumes<BetLost>,
        IConsumes<BetAdjusted>,
        IConsumes<BrandRegistered>,
        IConsumes<BankAccountAdded>,
        IConsumes<BankAccountDeactivated>,
        IConsumes<BankAccountEdited>,
        IConsumes<BankAdded>,
        IConsumes<BankEdited>,
        IConsumes<PlayerRegistrationChecked>,
        IConsumes<OnSiteMessageSentEvent>
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly PlayerCommands _playerCommands;

        public PlayerSubscriber(
            IPlayerRepository playerRepository, 
            PlayerCommands playerCommands)
        {
            _playerRepository = playerRepository;
            _playerCommands = playerCommands;
        }

        public void Consume(VipLevelRegistered @event)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var vipLevel = new VipLevel
                {
                    Id = @event.Id,
                    BrandId = @event.BrandId,
                    Code = @event.Code,
                    Name = @event.Name,
                    Rank = @event.Rank,
                    Description = @event.Description,
                    ColorCode = @event.ColorCode,
                    Status = @event.Status
                };

                _playerRepository.VipLevels.Add(vipLevel);
                _playerRepository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(VipLevelUpdated @event)
        {
            var vipLevel = _playerRepository.VipLevels.Single(x => x.Id == @event.Id);

            vipLevel.BrandId = @event.BrandId;
            vipLevel.Code = @event.Code;
            vipLevel.Name = @event.Name;
            vipLevel.Rank = @event.Rank;
            vipLevel.Description = @event.Description;
            vipLevel.ColorCode = @event.ColorCode;

            _playerRepository.SaveChanges();
        }

        public void Consume(BrandDefaultVipLevelChanged @event)
        {
            var brand = _playerRepository.Brands.Single(x => x.Id == @event.BrandId);
            brand.DefaultVipLevelId = @event.DefaultVipLevelId;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetWon @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotalWon += @event.Amount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetPlacedFree @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotalWon += @event.Amount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetLost @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotalLoss += @event.Amount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetAdjusted @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotlAdjusted += @event.Amount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BrandRegistered @event)
        {
            _playerRepository.Brands.Add(new Common.Data.Player.Brand
            {
                Id = @event.Id,
                Name = @event.Name,
                LicenseeId = @event.LicenseeId,
                TimezoneId = @event.TimeZoneId,
            });
            _playerRepository.SaveChanges();
        }

        private PlayerBetStatistics GetPlayerBetStatistics(Guid playerId)
        {
            var statistics = _playerRepository.PlayerBetStatistics
                .FirstOrDefault(s => s.PlayerId == playerId);

            if (statistics != null)
                return statistics;

            statistics = new PlayerBetStatistics
            {
                PlayerId = playerId,
            };
            _playerRepository.PlayerBetStatistics.Add(statistics);

            return statistics;
        }

        public void Consume(PlayerRegistrationChecked message)
        {
            var player = _playerRepository.Players.FirstOrDefault(x => x.Id == message.PlayerId);
            if (player == null)
                return;

            if (message.Action == SystemAction.FreezeAccount)
                _playerCommands.FreezeAccount(player.Id);

            if (message.Action == SystemAction.Deactivate)
                _playerCommands.SetStatus(player.Id, false);

            if (message.Action == SystemAction.NoAction)
                _playerCommands.SetStatus(player.Id, true);
        }

        public void Consume(BankAccountAdded message)
        {
            _playerRepository.BankAccounts.Add(new BankAccount
            {
                Id = message.Id,
                AccountId = message.AccountId,
                BankId = message.BankId,
                BankAccountStatus = message.BankAccountStatus
            });

            _playerRepository.SaveChanges();
        }

        public void Consume(BankAccountDeactivated message)
        {
            var bankAccount = _playerRepository.BankAccounts
                .Single(o => o.Id == message.Id);

            bankAccount.BankAccountStatus = BankAccountStatus.Pending;

            _playerRepository.BankAccounts.AddOrUpdate(bankAccount);
            _playerRepository.SaveChanges();
        }

        public void Consume(BankAccountEdited message)
        {
            var bankAccount = _playerRepository.BankAccounts
                .Single(o => o.Id == message.Id);

            bankAccount.AccountId = message.AccountId;
            bankAccount.BankId = message.BankId;
            bankAccount.BankAccountStatus = message.BankAccountStatus;

            _playerRepository.BankAccounts.AddOrUpdate(bankAccount);
            _playerRepository.SaveChanges();
        }

        public void Consume(BankAdded message)
        {
            _playerRepository.Banks.Add(new Bank
            {
                Id = message.Id,
                BankId = message.BankId,
                BankName = message.Name,
                BrandId = message.BrandId
            });

            _playerRepository.SaveChanges();
        }

        public void Consume(BankEdited message)
        {
            var bank = _playerRepository.Banks
                .Single(o => o.Id == message.Id);

            bank.BankName = message.BankName;
            bank.BrandId = message.BrandId;
            bank.BankId = message.BankId;

            _playerRepository.Banks.AddOrUpdate(bank);
            _playerRepository.SaveChanges();
        }

        public void Consume(OnSiteMessageSentEvent message)
        {
            var player = _playerRepository.Players
                .Include(x => x.OnSiteMessages)
                .Single(x => x.Id == message.PlayerId);

            player.OnSiteMessages.Add(new OnSiteMessage
            {
                Id = Guid.NewGuid(),
                Subject = message.Subject,
                Content = message.Content,
                Received = DateTimeOffset.UtcNow,
                IsNew = true
            });

            _playerRepository.SaveChanges();
        }
    }
}