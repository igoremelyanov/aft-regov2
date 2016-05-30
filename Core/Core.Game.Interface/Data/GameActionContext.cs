using System;
using AFT.UGS.Core.BaseModels.Enums;

namespace AFT.RegoV2.Core.Game.Interface.Data
{
    public class GameActionContext
    {
        public string GameProviderCode { get; set; }
        public string GameCode { get; set; }
        public PlatformType PlatformType { get; set; }

        public decimal TurnoverContribution { get; set; }
        public decimal GgrContribution { get; set; }
        public decimal UnsettledBetsContribution { get; set; }

        public string PlayerIpAddress { get; set; }
    }
}
