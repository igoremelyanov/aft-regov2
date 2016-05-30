using System;

namespace FakeUGS.Core.Data
{
    public class GameGroupGame
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        public Guid GameGroupId { get; set; }
        public virtual GameGroup GameGroup { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }

    }
}
