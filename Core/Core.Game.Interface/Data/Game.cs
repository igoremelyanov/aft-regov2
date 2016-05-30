using System;

using AFT.UGS.Core.BaseModels.Enums;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ExternalId { get; set; }

        public string EndpointPath { get; set; }

        public Guid GameProviderId { get; set; }
        public virtual GameProvider GameProvider { get; set; }

        public PlatformType PlatformType { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }

        public bool ZeroTurnover { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

    }
}
