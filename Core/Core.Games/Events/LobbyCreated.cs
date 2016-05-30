using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class LobbyCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public LobbyCreated()
        {
        }

        public LobbyCreated(Lobby lobby)
        {
            Id = lobby.Id;
            Name = lobby.Name;
            CreatedDate = lobby.CreatedDate;
            CreatedBy = lobby.CreatedBy;
        }
    }
}
