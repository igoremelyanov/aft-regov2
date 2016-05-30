using System;

namespace AFT.RegoV2.AdminApi.Interface.Player
{
    public class ChangePlayersPaymentLevelData
    {
        public Guid[] PlayerIds { get; set; }
        public Guid PaymentLevelId { get; set; }
        public string Remarks { get; set; }
    }
}
