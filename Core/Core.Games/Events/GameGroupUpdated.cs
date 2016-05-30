using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class GameGroupUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public GameGroupUpdated()
        {
            
        }

        public GameGroupUpdated(GameGroup gameGroup)
        {
            Id = gameGroup.Id;
            Name = gameGroup.Name;
            UpdatedDate = gameGroup.UpdatedDate.GetValueOrDefault();
            UpdatedBy = gameGroup.UpdatedBy;
        }
    }
}
