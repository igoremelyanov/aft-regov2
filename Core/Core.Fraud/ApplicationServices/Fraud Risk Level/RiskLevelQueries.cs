﻿using System;
using System.Linq;
using System.Data.Entity;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskLevelQueries : MarshalByRefObject, IRiskLevelQueries, IApplicationService
    {
        private readonly IFraudRepository _repository;

        public RiskLevelQueries(IFraudRepository repository)
        {
            this._repository = repository;
        }


        public RiskLevel GetById(Guid id)
        {
            return this._repository.RiskLevels.FirstOrDefault(x => x.Id == id);
        }

        [Permission(Permissions.View, Modules.FraudManager)]
        public IQueryable<RiskLevel> GetAll()
        {
            return this._repository.RiskLevels.Include(x => x.Brand);
        }

        public IQueryable<RiskLevel> GetByBrand(Guid brandId)
        {
            return this._repository.RiskLevels.Where(x => x.BrandId == brandId && x.Status == RiskLevelStatus.Active)
                .Include(x => x.Brand);
        }

        public IQueryable<PlayerRiskLevel> GetPlayerRiskLevels(Guid playerId)
        {
            return this._repository.PlayerRiskLevels.Where(x => x.PlayerId == playerId)
                  .Include(x => x.RiskLevel);
        }

        public IQueryable<PlayerRiskLevel> GetAllPlayerRiskLevels()
        {
            return this._repository.PlayerRiskLevels
                   .Include(x => x.RiskLevel);
        }
    }
}
