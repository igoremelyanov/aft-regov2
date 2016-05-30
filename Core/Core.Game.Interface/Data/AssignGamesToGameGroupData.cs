using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class AssignGamesToGameGroupData
    {
        public Guid GameGroupId { get; set; }

        public Guid[] GamesIds { get; set; }
    }
}
