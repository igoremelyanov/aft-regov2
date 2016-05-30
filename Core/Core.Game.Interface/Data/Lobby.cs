using System;
using System.Collections;
using System.Collections.Generic;

using AFT.UGS.Core.BaseModels.Enums;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class Lobby
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        
        public PlatformType PlatformType { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<BrandLobby> BrandLobbies { get; set; }
        public virtual ICollection<GameGroup> GameGroups { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}
