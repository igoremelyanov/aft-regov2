using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class LobbyUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public LobbyUpdated()
        {
            
        }

        public LobbyUpdated(Lobby lobby)
        {
            Id = lobby.Id;
            Name = lobby.Name;
            UpdatedDate = lobby.UpdatedDate.GetValueOrDefault();
            UpdatedBy = lobby.UpdatedBy;
        }
    }
}
