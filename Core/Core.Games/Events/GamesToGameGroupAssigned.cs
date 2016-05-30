using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Game.Events
{
    public class GamesToGameGroupAssigned : DomainEventBase
    {
        public GamesToGameGroupAssigned() { }

        public GamesToGameGroupAssigned(Guid gameGroupId, string name, IEnumerable<Guid> gamesIds)
        {
            GameGroupId = gameGroupId;
            Name = name;
            GamesIds = gamesIds.ToList();
        }

        public Guid GameGroupId { get; set; }
        public string Name { get; set; }
        public List<Guid> GamesIds { get; set; }
    }
}
