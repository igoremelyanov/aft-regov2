using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class AssignLobbiesToBrandData
    {
        public Guid BrandId { get; set; }

        public Guid[] LobbiesIds { get; set; }
    }
}
