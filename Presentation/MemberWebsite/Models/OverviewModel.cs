using System;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class OverviewModel
    {
        public string BonusCode { get; set; }
        public string BonusName { get; set; }
        public Guid? BonusId { get; set; }
        public GamesDataView Games { get; set; }
        public string MessageSubject { get; set; }
        public Guid? MessageId { get; set; }
    }
}