using System;

namespace AFT.RegoV2.Bonus.Api.Interface.Responses
{
    public class BonusTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public bool RequireBonusCode { get; set; }
    }

    public class AddEditBonusResponse: ValidationResponseBase
    {
        public Guid? BonusId { get; set; }
    }

    public class ToggleBonusStatusResponse : ValidationResponseBase
    {
    }
}
