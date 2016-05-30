using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.GameProvider;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class GamesDataView
    {
        public bool IsAuthenticated { get; set; }

        public GamesResponse Data { get; set; } 
    }
}
