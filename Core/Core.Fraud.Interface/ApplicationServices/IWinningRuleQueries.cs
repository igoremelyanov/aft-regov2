using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IWinningRuleQueries
    {
        IEnumerable<WinningRule> GetWinningRules(Guid avcId);
    }
}