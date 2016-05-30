using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands
{
    public class SearchPlayersRequest
    {
        public Guid BrandId { get; set; }
        public string SearchTerm { get; set; }
        public Guid? PaymentLevelId { get; set; }
        public Guid? VipLevelId { get; set; }
        public Status? PlayerStatus { get; set; }
        public DateTimeOffset? RegistrationDateFrom { get; set; }
        public DateTimeOffset? RegistrationDateTo { get; set; }
    }
}