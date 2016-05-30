using System;

namespace FakeUGS.Core.Data
{
    public class AssignGamesToGameGroupData
    {
        public Guid GameGroupId { get; set; }

        public Guid[] GamesIds { get; set; }
    }
}
