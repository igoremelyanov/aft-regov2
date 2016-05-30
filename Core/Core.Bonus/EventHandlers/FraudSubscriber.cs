using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;
using RiskLevel = AFT.RegoV2.Bonus.Core.Data.RiskLevel;

namespace AFT.RegoV2.Bonus.Core.EventHandlers
{
    public class FraudSubscriber
    {
        private readonly IUnityContainer _container;
        private const string NoPlayerFormat = "No player found with Id: {0}";
        private const string NoRiskLevelFormat = "No risk level found with Id: {0}";

        public FraudSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(RiskLevelTagPlayer @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var player = bonusRepository.Players.SingleOrDefault(x => x.Id == @event.PlayerId);
            if (player == null)
                throw new RegoException(string.Format(NoPlayerFormat, @event.PlayerId));

            var riskLevel = player.Brand.RiskLevels.SingleOrDefault(x => x.Id == @event.RiskLevelId);
            if (riskLevel == null)
                throw new RegoException(string.Format(NoRiskLevelFormat, @event.RiskLevelId));

            if (!player.RiskLevels.Exists(rl => rl.Id == @event.RiskLevelId))
            {
                player.RiskLevels.Add(riskLevel);
                bonusRepository.SaveChanges();
            }
        }

        public void Handle(RiskLevelUntagPlayer @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var player = bonusRepository.Players.FirstOrDefault(x => x.Id == @event.PlayerId);
            if (player == null)
                throw new RegoException(string.Format(NoPlayerFormat, @event.PlayerId));

            var riskLevel = player.RiskLevels.FirstOrDefault(rl => rl.Id == @event.RiskLevelId);
            if (riskLevel == null)
                throw new RegoException($"Player (Id: {@event.PlayerId}) is not tagged by risk level Id: {@event.RiskLevelId}");

            player.RiskLevels.Remove(riskLevel);
            bonusRepository.SaveChanges();
        }

        public void Handle(RiskLevelStatusUpdated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var riskLevel = bonusRepository.Brands.SelectMany(b => b.RiskLevels).SingleOrDefault(x => x.Id == @event.Id);
            if (riskLevel == null)
                throw new RegoException(string.Format(NoRiskLevelFormat, @event.Id));

            riskLevel.IsActive = @event.NewStatus == RiskLevelStatus.Active;
            bonusRepository.SaveChanges();
        }

        public void Handle(RiskLevelCreated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(x => x.Id == @event.BrandId);
            if (brand == null)
                throw new RegoException($"No brand found with Id: {@event.BrandId}");

            var newRiskLevel = new RiskLevel
            {
                Id = @event.Id,
                IsActive = @event.Status == RiskLevelStatus.Active
            };

            brand.RiskLevels.Add(newRiskLevel);
            bonusRepository.SaveChanges();
        }

        public void Handle(PlayerRegistrationChecked @event)
        {
            if (@event.Action != SystemAction.DisableBonus)
                return;

            var bonusRepository = _container.Resolve<IBonusRepository>();

            var player = bonusRepository.GetLockedPlayer(@event.PlayerId);
            player.DisableBonuses();
            bonusRepository.SaveChanges();
        }
    }
}