using System.Collections.Generic;
using AFT.RegoV2.MemberApi.Interface.Bonus;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class RegisterStep2Model : MemberApi.Interface.Payment.RegisterStep2Model
    {
        public string AmountFormatted { get; set; }
        public string MinFormatted { get; set; }
        public string MaxFormatted { get; set; }
        public IEnumerable<QuickSelectAmount> QuickSelectAmounts { get; set; }
    }

    public class RegisterStep3Model
    {
        public string BrandName { get; set; }
        public decimal DepositAmount { get; set; }
        public IEnumerable<QualifiedBonus> Bonuses { get; set; }
    }

    public class RegisterStep4Model
    {
        public string BonusCode { get; set; }
        public decimal? DepositAmount { get; set; }
        public string DepositAmountFormatted { get; set; }
        public decimal? BonusAmount { get; set; }
        public string BonusAmountFormatted { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountFormatted { get; set; }
    }
}