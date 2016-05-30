using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class BrandLobby
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        public Guid LobbyId { get; set; }
        public virtual Lobby Lobby { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }

    }
}
