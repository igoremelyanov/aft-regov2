using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Events.Management;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class BonusSubscriber :
        IConsumes<BonusActivated>,
        IConsumes<BonusDeactivated>,
        IConsumes<BonusCreated>
    {
        private readonly IFraudRepository _repository;

        public BonusSubscriber(IFraudRepository repository)
        {
            _repository = repository;
        }

        public void Consume(BonusActivated message)
        {
            var bonus = _repository.Bonuses
                .Single(o => o.Id == message.AggregateId);

            bonus.IsActive = true;

            _repository.SaveChanges();
        }

        public void Consume(BonusDeactivated message)
        {
            var bonus = _repository.Bonuses
                .Single(o => o.Id == message.AggregateId);

            bonus.IsActive = false;

            _repository.SaveChanges();
        }

        public void Consume(BonusCreated message)
        {
            _repository.Bonuses.Add(new Interface.Data.Bonus
            {
                Id = message.AggregateId,
                Name = message.Name,
                Code = message.Code,
                IsActive = message.IsActive,
                BonusType = (BonusType) message.BonusType,
                BrandId = message.BrandId
            });

            _repository.SaveChanges();
        }
    }
}
