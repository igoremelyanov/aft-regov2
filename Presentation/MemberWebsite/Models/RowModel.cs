using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface.Bonus;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class BonusRowModel
    {
        public bool IsFirstRow { get; set; }
        public bool slider { get; set; }
        public IEnumerable<QualifiedBonus> Bonuses { get; set; } 
    }
}
