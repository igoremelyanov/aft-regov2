using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class OfflineWithdrawResponse : IApplicationService
    {
        public Guid Id { get; set; }
    }
}