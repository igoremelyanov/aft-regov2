using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskLevelCommands : MarshalByRefObject, IRiskLevelCommands, IApplicationService
    {
        private readonly IFraudRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;

        public RiskLevelCommands(IEventBus serviceBus, IFraudRepository repository, IActorInfoProvider actorInfoProvider)
        {
            _eventBus = serviceBus;
            _repository = repository;
            _actorInfoProvider = actorInfoProvider;
        }


        private void UpdateStatus(Guid id, bool isActive, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = _repository.RiskLevels.SingleOrDefault(x => x.Id == id);

                if (riskLevel == null)
                    throw new ArgumentException("app:fraud.manager.message.invalidRiskLevelId");

                riskLevel.Status = isActive ? RiskLevelStatus.Active : RiskLevelStatus.Inactive;
                riskLevel.Description = remarks;

                _repository.SaveChanges();

                _eventBus.Publish(new RiskLevelStatusUpdated(id, riskLevel.Status));

                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Modules.FraudManager)]
        public void Activate(RiskLevelId id, string remarks)
        {
            UpdateStatus(id, true, remarks);
        }

        [Permission(Permissions.Deactivate, Modules.FraudManager)]
        public void Deactivate(RiskLevelId id, string remarks)
        {
            UpdateStatus(id, false, remarks);
        }

        [Permission(Permissions.Update, Modules.FraudManager)]
        public void Update(RiskLevel data)
        {
            var validationResult = new UpdateRiskLevelValidator(_repository)
               .Validate(data);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = _repository.RiskLevels.Single(x => x.Id == data.Id);

                riskLevel.BrandId = data.BrandId;
                riskLevel.Level = data.Level;
                riskLevel.Name = data.Name;
                riskLevel.Status = data.Status;
                riskLevel.Description = data.Description;
                riskLevel.DateUpdated = DateTimeOffset.Now;
                riskLevel.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new RiskLevelUpdated(data.Id, data.BrandId, data.Level, data.Name, data.Status, data.Description));

                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Modules.FraudManager)]
        public void Create(RiskLevel data)
        {
            if (data.Id == Guid.Empty)
            {
                data.Id = Guid.NewGuid();
                data.Status = RiskLevelStatus.Inactive;
            }

            var validationResult = new CreateRiskLevelValidator(_repository)
                .Validate(data);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                data.CreatedBy = _actorInfoProvider.Actor.UserName;
                data.DateCreated = DateTimeOffset.Now;

                _repository.RiskLevels.Add(data);
                _repository.SaveChanges();

                _eventBus.Publish(new RiskLevelCreated(data.Id, data.BrandId, data.Level, data.Name, data.Status, data.Description));

                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Modules.FraudManager)]
        public void Tag(PlayerId playerId, RiskLevelId riskLevel, string description)
        {
            //TODO: validate
            if (_repository.PlayerRiskLevels.Any(x => x.RiskLevelId == riskLevel && x.PlayerId == playerId))
                throw new RegoValidationException("Already tagged with this Fraud Risk Level.");

            Guid id = Guid.NewGuid();

            var domain = new Entities.RiskLevel();
            domain.TagPlayer(id, playerId, riskLevel, description);

            domain.Events.ForEach(ev => _eventBus.Publish(ev));
        }

        [Permission(Permissions.Update, Modules.FraudManager)]
        public void Untag(PlayerId id, string description)
        {
            //TODO: validate

            var domain = new Entities.RiskLevel();

            var playerRiskLevel =_repository.PlayerRiskLevels.FirstOrDefault(x => x.Id == id);
            if (playerRiskLevel == null)
                throw new ArgumentException("app:fraud.manager.message.invalidPlayerRiskLevelId");

            domain.UntagPlayer(id, playerRiskLevel.PlayerId, playerRiskLevel.RiskLevelId, description);

            domain.Events.ForEach(ev => _eventBus.Publish(ev));
        }
    }
}
