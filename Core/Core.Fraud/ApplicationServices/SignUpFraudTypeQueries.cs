using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class SignUpFraudTypeQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IFraudRepository _fraudRepository;

        public SignUpFraudTypeQueries(IFraudRepository fraudRepository)
        {
            _fraudRepository = fraudRepository;
        }

        [Permission(Permissions.View, Module = Modules.SignUpFraudTypes)]
        public SignUpFraudType GetById(Guid id)
        {
            return _fraudRepository.SignUpFraudTypes
                         .Include(o => o.RiskLevels)
                         .Single(o => o.Id == id);
        }

        [Permission(Permissions.View, Module = Modules.SignUpFraudTypes)]
        public IEnumerable<RiskLevel> GetActiveRiskLevels(IEnumerable<Guid> allowedBrands)
        {
            return _fraudRepository.RiskLevels
                .Where(o => o.Status == RiskLevelStatus.Active)
                .Where(level => allowedBrands.Contains(level.BrandId));
        }

        [Permission(Permissions.View, Module = Modules.SignUpFraudTypes)]
        public IEnumerable<SignUpFraudType> GetSignUpFraudTypes()
        {
            return _fraudRepository.SignUpFraudTypes.Include(o => o.RiskLevels).ToList();
        }
    }
}
