using System;
using AFT.RegoV2.Core.Common.Data.Player.Enums;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class SendNewPasswordData
    {
        public Guid PlayerId { get; set; }
        public string NewPassword { get; set; }
        public SendBy SendBy { get; set; }
    }
}