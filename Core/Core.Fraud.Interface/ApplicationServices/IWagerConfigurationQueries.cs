using System;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IWagerConfigurationQueries
    {
        IQueryable<WagerConfigurationDTO> GetWagerConfigurations();
        WagerConfigurationDTO GetWagerConfiguration(Guid id);
    }
}