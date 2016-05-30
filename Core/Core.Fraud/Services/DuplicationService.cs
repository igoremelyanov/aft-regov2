using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public interface IDuplicationService
    {
        void ApplyAction(Guid playerId);
    }

    public class DuplicationService : IDuplicationService
    {
        private readonly IEventBus _bus;
        private readonly IUnityContainer _container;

        public DuplicationService(
            IEventBus bus, IUnityContainer container)
        {
            _bus = bus;
            _container = container;
        }

        public void ApplyAction(Guid playerId)
        {
            var repository = _container.Resolve<IFraudRepository>();
            var duplicateScoreService = _container.Resolve<IDuplicateScoreService>();

            var player = repository.Players.FirstOrDefault(x => x.Id == playerId);
            if (player == null)
                return;
            var score = duplicateScoreService.ScorePlayer(playerId);

            var configuration =
                repository
                    .DuplicateMechanismConfigurations
                    .FirstOrDefault(x => x.BrandId == player.BrandId);
            if (configuration == null)
                return;

            SystemAction action = SystemAction.NoAction;
            QueueFolderTag tag = QueueFolderTag.NoHandling;

            if (score > configuration.NoHandlingScoreMin && score <= configuration.NoHandlingScoreMax)
            {
                action = configuration.NoHandlingSystemAction;
                tag = QueueFolderTag.NoHandling;
            }
            else if (score > configuration.FraudulentScoreMin && score <= configuration.FraudulentScoreMax)
            {
                action = configuration.FraudulentSystemAction;
                tag = QueueFolderTag.Fraudlent;
            }
            else if (score > configuration.RecheckScoreMin && score <= configuration.RecheckScoreMax)
            {
                action = configuration.RecheckSystemAction;
                tag = QueueFolderTag.ReCheck;
            }

            SendStatusUpdate(playerId, action, tag);
        }

        private void SendStatusUpdate(Guid playerId, SystemAction action, QueueFolderTag tag)
        {
            var @event = new PlayerRegistrationChecked
            {
                DateChecked = DateTimeOffset.UtcNow,
                Action = action,
                Tag = tag,
                PlayerId = playerId
            };

            _bus.Publish(@event);
        }
    }
}
