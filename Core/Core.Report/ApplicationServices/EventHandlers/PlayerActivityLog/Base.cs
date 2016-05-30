using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class PlayerActivityLogEventHandlersBase
    {
        protected readonly IUnityContainer Container;

        protected string Category;

        protected PlayerActivityLogEventHandlersBase(IUnityContainer container)
        {
            Container = container;
        }

        protected void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId,
            Dictionary<string, object> activityValues)
        {
            AddActivityLog(activityName, @event, playerId, activityValues == null ? string.Empty :
                    string.Join("; ", activityValues.Select(v => v.Key + ": " + v.Value)));
        }

        protected string GetPlayerName(Guid playerId)
        {
            var repository = Container.Resolve<IPlayerRepository>();
            var player = repository.Players.SingleOrDefault(p => p.Id == playerId);

            return player == null ? string.Empty : player.Username;
        }

        protected void AddActivityLog(string activityName, IDomainEvent @event, Guid playerId, string remarks = "")
        {
            var repository = Container.Resolve<IPlayerRepository>();

            repository.PlayerActivityLog.Add(new PlayerActivityLog
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Category = Category,
                ActivityDone = activityName,
                DatePerformed = @event.EventCreated,
                PerformedBy = @event.EventCreatedBy,
                Remarks = remarks
            });
            repository.SaveChanges();
        }
    }
}
