using AFT.RegoV2.Core.Game.Interface.Data;
using System;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class GameProviderConfiguration
    {
        public Guid Id { get; set; }
        public virtual GameProvider GameProvider { get; set; }
        public Guid GameProviderId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Endpoint { get; set; }
        public string Type { get; set; }

        public string AuthorizationClientId { get; set; }
        public string AuthorizationSecret { get; set; }

        public string SecurityKey { get; set; }
        public DateTimeOffset? SecurityKeyExpiryTime { get; set; }

        public AuthenticationMethod Authentication { get; set; }

        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}