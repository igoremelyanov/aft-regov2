using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Game.Events
{
    public class LobbiesToBrandAssigned : DomainEventBase
    {
        public LobbiesToBrandAssigned() { }

        public LobbiesToBrandAssigned(Guid brandId, string name, IEnumerable<Guid> lobbiesIds)
        {
            BrandId = brandId;
            Name = name;
            LobbiesIds = lobbiesIds.ToList();
        }

        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public List<Guid> LobbiesIds { get; set; }
    }
}
