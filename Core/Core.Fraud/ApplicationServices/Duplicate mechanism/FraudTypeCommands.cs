using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices.Duplicate_mechanism
{
    public class FraudTypeCommands : IFraudTypeCommands
    {
        private readonly IFraudRepository _repository;
        private readonly IEventBus _bus;

        public FraudTypeCommands(IFraudRepository repository, IEventBus bus)
        {
            _repository = repository;
            _bus = bus;
        }

        public void UpdatePlayer(SignupUpdateData data)
        {
            if (data.Action == SignupUpdateAction.Remove)
            {
                foreach (var item in data.Data)
                {
                    var fraudType = _repository.SignUpFraudTypes.Single(x => x.Id == item.FraudTypeId);
                    UpdatePlayer(SystemAction.NoAction, item.PlayerId, fraudType.Name, data.Remarks);
                }
            }

            if (data.Action == SignupUpdateAction.Apply)
            {
                foreach (var item in data.Data)
                {
                    var fraudType = _repository.SignUpFraudTypes.Single(x => x.Id == item.FraudTypeId);
                    UpdatePlayer(fraudType.SystemAction, item.PlayerId, fraudType.Name, data.Remarks);
                }
            }

            if (data.Action == SignupUpdateAction.New)
            {
                foreach (var item in data.Data)
                {
                    var fraudType = _repository.SignUpFraudTypes.Single(x => x.Id == item.FraudTypeId);
                    UpdatePlayer(data.Sanction.Value, item.PlayerId, fraudType.Name, data.Remarks);
                }
            }
        }

        private void UpdatePlayer(SystemAction action, Guid playerId, string fraudTypeName, string remarks)
        {
            var player = _repository.Players.Single(x => x.Id == playerId);
            player.FolderAction = action;
            player.HandledDate = DateTimeOffset.UtcNow;
            player.CompletedDate = DateTimeOffset.UtcNow;
            player.FraudType = fraudTypeName;
            player.SignUpRemark = remarks;

            _repository.SaveChanges();

            var @event = new PlayerRegistrationChecked
            {
                DateChecked = DateTimeOffset.UtcNow,
                Action = action,
                Tag = QueueFolderTag.Completed,
                PlayerId = playerId
            };

            _bus.Publish(@event);

        }

    }
}