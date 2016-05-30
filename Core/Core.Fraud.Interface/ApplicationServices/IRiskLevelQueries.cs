using System;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IRiskLevelQueries
    {
        RiskLevel GetById(Guid id);
        IQueryable<RiskLevel> GetAll();
        IQueryable<RiskLevel> GetByBrand(Guid brandId);
        IQueryable<PlayerRiskLevel> GetAllPlayerRiskLevels();
        IQueryable<PlayerRiskLevel> GetPlayerRiskLevels(Guid playerId);
    }
}
