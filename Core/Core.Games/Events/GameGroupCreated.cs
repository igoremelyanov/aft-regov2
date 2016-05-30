using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class GameGroupCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public GameGroupCreated()
        {
        }

        public GameGroupCreated(GameGroup gameGroup)
        {
            Id = gameGroup.Id;
            Name = gameGroup.Name;
            Code = gameGroup.Code;
            CreatedDate = gameGroup.CreatedDate;
            CreatedBy = gameGroup.CreatedBy;
        }
    }
}
