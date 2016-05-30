using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IPlayerQueries : IApplicationService
    {
        ValidationResult GetValidationFailures(string username, string password);
        Player GetPlayer(Guid playerId);
        Task<Player> GetPlayerAsync(PlayerId playerId);
        Player GetPlayerForWithdraw(PlayerId playerId);
        IList<VipLevel> VipLevels { get; }
        IQueryable<Player> GetPlayersByVipLevel(Guid vipLevelId);
        VipLevel GetDefaultVipLevel(Guid brandId);
        Player GetPlayerByUsername(string username);
        Player GetPlayerByEmail(string email);
        SecurityQuestion GetSecurityQuestion(Guid playerId);
        Player GetPlayerByResetPasswordToken(string token);
        IQueryable<Player> GetPlayers();
        OnSiteMessage GetOnSiteMessage(Guid onSiteMessageId);
        IEnumerable<OnSiteMessage> GetOnSiteMessages(Guid playerId);
        int GetOnSiteMessagesCount(Guid playerId);
    }


}
