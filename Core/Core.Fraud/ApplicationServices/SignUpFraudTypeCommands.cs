using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Events;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class SignUpFraudTypeCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IFraudRepository _fraudRepository;
        private readonly IEventBus _eventBus;

        public SignUpFraudTypeCommands(IFraudRepository fraudRepository, IEventBus eventBus)
        {
            _fraudRepository = fraudRepository;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Create, Module = Modules.SignUpFraudTypes)]
        public void Create(SignUpFraudTypeDTO data, string createdBy)
        {
            var levels = new List<RiskLevel>();
            if (data.RiskLevels != null)
                data.RiskLevels.ForEach(id =>
                        levels.Add(
                            _fraudRepository.RiskLevels.Single(l => l.Id == id)
                        )
                    );

            var signUpFraudType = new SignUpFraudType
            {
                Id = Guid.NewGuid(),
                Name = data.FraudTypeName,
                SystemAction = data.SystemAction,
                RiskLevels = levels,
                Remarks = data.Remarks,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = createdBy
            };

            _fraudRepository.SignUpFraudTypes.Add(signUpFraudType);

            _fraudRepository.SaveChanges();

            _eventBus.Publish(new SignUpFraudTypeCreated
            {
                Id = signUpFraudType.Id,
                FraudTypeName = signUpFraudType.Name,
                SystemAction = signUpFraudType.SystemAction.ToString(),
                RiskLevels = signUpFraudType.RiskLevels.Select(o => o.Name).ToArray(),
                Remarks = signUpFraudType.Remarks
            });
        }

        [Permission(Permissions.Update, Module = Modules.SignUpFraudTypes)]
        public void Update(SignUpFraudTypeDTO data, string updatedBy)
        {
            var entity = _fraudRepository.SignUpFraudTypes
                        .Include(o => o.RiskLevels)
                        .Single(o => o.Id == data.Id);

            entity.RiskLevels.Clear();
            var levels = new List<RiskLevel>();
            if (data.RiskLevels != null)
                data.RiskLevels.ForEach(id =>
                        levels.Add(
                            _fraudRepository.RiskLevels.Single(l => l.Id == id)
                        )
                    );

            entity.Name = data.FraudTypeName;
            entity.SystemAction = data.SystemAction;
            entity.RiskLevels = levels;
            entity.Remarks = data.Remarks;
            entity.DateUpdated = DateTimeOffset.Now;
            entity.UpdatedBy = updatedBy;

            _fraudRepository.SaveChanges();

            _eventBus.Publish(new SignUpFraudTypeUpdated
            {
                Id = entity.Id,
                FraudTypeName = entity.Name,
                SystemAction = entity.SystemAction.ToString(),
                RiskLevels = entity.RiskLevels.Select(o => o.Name).ToArray(),
                Remarks = entity.Remarks
            });
        }
    }
}
