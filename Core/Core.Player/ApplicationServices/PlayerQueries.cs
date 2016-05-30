using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class PlayerQueries : MarshalByRefObject, IPlayerQueries
    {
        private readonly IPlayerRepository _repository;
        private readonly BrandQueries _brandQueries;
        private readonly IAuthQueries _authQueries;

        static PlayerQueries()
        {
            AutoMapper.Mapper.CreateMap<OnSiteMessage, Interface.Data.OnSiteMessage>();
        }

        public PlayerQueries(IPlayerRepository repository, BrandQueries brandQueries, IAuthQueries authQueries)
        {
            _repository = repository;
            _brandQueries = brandQueries;
            _authQueries = authQueries;
        }

        public ValidationResult GetValidationFailures(string username, string password)
        {
            var player = _repository.Players.SingleOrDefault(x => x.Username == username);
            var playerValidationResult = new LoginValidator(_brandQueries, _authQueries, password).Validate(player);

            return playerValidationResult;
        }

        public IQueryable<Common.Data.Player.Player> GetPlayersByVipLevel(Guid vipLevelId)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .Where(o => o.VipLevel.Id == vipLevelId);
        }

        public VipLevel GetDefaultVipLevel(Guid brandId)
        {
            var defaultVipLevel = _repository.Brands.Where(x => x.Id == brandId).Select(x => x.DefaultVipLevel).Single();
            if (defaultVipLevel == null)
                throw new RegoException(string.Format("Default VipLevel was not found for brand '{0}'", brandId));
            return defaultVipLevel;
        }

        public IQueryable<Common.Data.Player.Player> GetPlayersByPaymentLevel(Guid paymentLevelId)
        {
            return _repository.Players.Where(x => x.PaymentLevelId == paymentLevelId);
        }

        public Common.Data.Player.Player GetPlayerByUsername(string username)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.Username == username);
        }

        public Common.Data.Player.Player GetPlayerByEmail(string email)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.Email == email);
        }

        public Common.Data.Player.Player GetPlayerByResetPasswordToken(string token)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.ResetPasswordToken == token);
        }

        [Permission(Permissions.View, Module = Modules.PlayerManager)]
        public Common.Data.Player.Player GetPlayer(Guid playerId)
        {
            return _repository.Players
                .Include(x => x.IdentityVerifications)
                .Include(x => x.VipLevel)
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == playerId);
        }

        [Permission(Permissions.View, Module = Modules.PlayerManager)]
        public async Task<Common.Data.Player.Player> GetPlayerAsync(PlayerId playerId)
        {
            return await _repository.Players
                .Include(x => x.Brand)
                .Include(x => x.VipLevel)
                .SingleOrDefaultAsync(x => x.Id == playerId);
        }

        [Permission(Permissions.Create, Module = Modules.OfflineWithdrawalRequest)]
        public Common.Data.Player.Player GetPlayerForWithdraw(PlayerId playerId)
        {
            return GetPlayer(playerId);
        }

        [Permission(Permissions.View, Module = Modules.PlayerManager)]
        public IQueryable<Common.Data.Player.Player> GetPlayers()
        {
            return _repository.Players.Include(x => x.VipLevel).AsNoTracking();
        }

        public IList<VipLevel> VipLevels
        {
            get { return _repository.VipLevels.ToList(); }
        }

        public IEnumerable<PlayerInfoLog> GetPlayerInfoLog(Guid playerId)
        {
            return _repository.PlayerInfoLog
                .Include(p => p.Player)
                .Where(p => p.Player.Id == playerId)
                .OrderByDescending(p => p.RowVersion);
        }

        public IQueryable<IdentityVerification> GetPlayerIdentityVerifications(Guid playerId)
        {
            var player = _repository.Players
                .Include(x => x.IdentityVerifications)
                .SingleOrDefault(x => x.Id == playerId);

            return player == null
                ? null
                : player.IdentityVerifications.AsQueryable();
        }

        public IQueryable<PlayerActivityLog> GetPlayerActivityLog()
        {
            return _repository.PlayerActivityLog;
        }

        public IQueryable<PlayerBetStatistics> GetPlayerBetStatistics()
        {
            return _repository.PlayerBetStatistics;
        }

        public IQueryable<SecurityQuestion> GetSecurityQuestions()
        {
            return _repository.SecurityQuestions;
        }

        public IEnumerable<BankAccount> GetBankAccounts(Guid brandId)
        {
            return _repository.BankAccounts
                .Include(o => o.Bank)
                .Where(o => o.Bank.BrandId == brandId)
                .ToList();
        }

        public SecurityQuestion GetSecurityQuestion(Guid playerId)
        {
            var securityQuestionId = GetPlayer(playerId).SecurityQuestionId;
            return GetSecurityQuestions().Single(q => q.Id == securityQuestionId);
        }

        public Interface.Data.OnSiteMessage GetOnSiteMessage(Guid onSiteMessageId)
        {
            var onSiteMessage = _repository.OnSiteMessages
                .Single(x => x.Id == onSiteMessageId);

            var result = AutoMapper.Mapper.Map<Interface.Data.OnSiteMessage>(onSiteMessage);

            if (onSiteMessage.IsNew)
            {
                onSiteMessage.IsNew = false;
                _repository.SaveChanges();
            }

            return result;
        }

        public IEnumerable<Interface.Data.OnSiteMessage> GetOnSiteMessages(Guid playerId)
        {
            var onSiteMessages = _repository.Players
                .Include(x => x.OnSiteMessages)
                .Single(x => x.Id == playerId)
                .OnSiteMessages
                .Where(o => o.Received >= o.Received.AddYears(-1))
                .OrderByDescending(x => x.Received)
                .Take(20);

            var result = AutoMapper.Mapper.Map<List<Interface.Data.OnSiteMessage>>(onSiteMessages);

            return result;
        }

        public int GetOnSiteMessagesCount(Guid playerId)
        {
            var count = _repository.Players
                .Include(x => x.OnSiteMessages)
                .Single(x => x.Id == playerId)
                .OnSiteMessages
                .Count(o => o.IsNew);

            return count;
        }
    }
}