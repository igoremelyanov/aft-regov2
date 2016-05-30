using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IOnlineDepositQueries
    {
        CheckStatusResponse CheckStatus(CheckStatusRequest commandRequest);
        DepositDto GetOnlineDepositById(Guid id);
        IEnumerable<DepositDto> GetOnlineDepositsByPlayerId(Guid playerId);
    }
}