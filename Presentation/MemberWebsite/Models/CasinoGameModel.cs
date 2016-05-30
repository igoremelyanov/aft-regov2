using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.GameProvider;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class CasinoGameModel
    {
        public CasinoGameModel(GameDto game, string cdnRoot, string iconResolution)
        {
            Game = game;
            CdnRoot = cdnRoot;
            IconResolution = iconResolution;
        }

        public GameDto Game { get; private set; }
        public string CdnRoot { get; private set; }
        public string IconResolution { get; private set; }
    }
}
