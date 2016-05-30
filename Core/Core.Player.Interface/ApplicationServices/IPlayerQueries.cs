using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Player.Interface.ApplicationServices
{
    public interface IPlayerQueries : IApplicationService
    {
        ValidationResult GetValidationFailures(string username, string password);
        Common.Data.Player.Player GetPlayer(Guid playerId);
        Task<Common.Data.Player.Player> GetPlayerAsync(PlayerId playerId);
        Common.Data.Player.Player GetPlayerForWithdraw(PlayerId playerId);
        IList<VipLevel> VipLevels { get; }
        IQueryable<Common.Data.Player.Player> GetPlayersByVipLevel(Guid vipLevelId);
        VipLevel GetDefaultVipLevel(Guid brandId);
        Common.Data.Player.Player GetPlayerByUsername(string username);
        Common.Data.Player.Player GetPlayerByEmail(string email);
        SecurityQuestion GetSecurityQuestion(Guid playerId);
        Common.Data.Player.Player GetPlayerByResetPasswordToken(string token);
        IQueryable<Common.Data.Player.Player> GetPlayers();
        Data.OnSiteMessage GetOnSiteMessage(Guid onSiteMessageId);
        IEnumerable<Data.OnSiteMessage> GetOnSiteMessages(Guid playerId);
        int GetOnSiteMessagesCount(Guid playerId);
    }
}
