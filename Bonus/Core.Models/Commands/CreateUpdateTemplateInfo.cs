using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CreateUpdateTemplateInfo
    {
        public Guid? BrandId { get; set; }
        public string Name { get; set; }
        public BonusType TemplateType { get; set; }
        public string Description { get; set; }
        public Guid WalletTemplateId { get; set; }
        public bool IsWithdrawable { get; set; }
        public IssuanceMode Mode { get; set; }
    }
}