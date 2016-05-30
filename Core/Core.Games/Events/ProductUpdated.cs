using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class ProductUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public ProductUpdated()
        {
            
        }

        public ProductUpdated(GameProvider gameProvider)
        {
            Id = gameProvider.Id;
            Name = gameProvider.Name;
            Code = gameProvider.Code;
            UpdatedDate = gameProvider.UpdatedDate.GetValueOrDefault();
            UpdatedBy = gameProvider.UpdatedBy;
        }
    }
}
