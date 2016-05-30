using System;
using System.Collections.Generic;

namespace FakeUGS.Core.Data
{
    public class GameGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public Guid LobbyId { get; set; }
        public virtual Lobby Lobby { get; set; }

        public virtual ICollection<GameGroupGame> GameGroupGames { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

    }
}
