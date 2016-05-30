using AFT.RegoV2.Core.Game.Interface.Data;
using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class GameProvider
    {
        public Guid     Id { get; set; }
        public string   Name { get; set; }
        public string   Code { get; set; }
        public bool     IsActive { get; set; }
        
        public GameProviderCategory Category { get; set; }


        public string           CreatedBy { get; set; }
        public DateTimeOffset   CreatedDate { get; set; }
        public string           UpdatedBy { get; set; }
        public DateTimeOffset?  UpdatedDate { get; set; }

        public virtual ICollection<GameProviderConfiguration>    GameProviderConfigurations { get; set; }
        public ICollection<GameProviderCurrency> GameProviderCurrencies { get; set; }
        public ICollection<Game> Games { get; set; }
    }
}
