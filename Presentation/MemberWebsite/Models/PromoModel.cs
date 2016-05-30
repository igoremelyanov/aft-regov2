using System;
using AFT.RegoV2.MemberApi.Interface.Bonus;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class PromoModel
    {
        public QualifiedBonus Bonus { get; set; }
        public Guid? NextId { get; set; }
        public Guid? PreviousId { get; set; }
        public bool PlayerHaveActiveBonus { get; set; }
    }
}