using System;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IRiskProfileCheckQueries
    {
        IQueryable<RiskProfileConfiguration> GetConfigurations();
        RiskProfileCheckDTO GetConfiguration(Guid id);
        object GetVipLevels(Guid? brandId);
    }
}