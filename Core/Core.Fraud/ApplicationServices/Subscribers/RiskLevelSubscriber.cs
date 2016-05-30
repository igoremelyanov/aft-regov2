using System.Linq;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using LocalBrand = AFT.RegoV2.Core.Fraud.Interface.Data.Brand;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskLevelSubscriber : IBusSubscriber,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<RiskLevelTagPlayer>,
        IConsumes<RiskLevelUntagPlayer>
    {
        private readonly IFraudRepository _repository;

        public RiskLevelSubscriber(IFraudRepository repository)
        {
            _repository = repository;
        }



        public void Consume(BrandRegistered @event)
        {
            if (!_repository.Brands.Any(x => x.Id == @event.Id))
            {
                _repository.Brands.Add(new LocalBrand
                {
                    Id = @event.Id,
                    Name = @event.Name,
                    Code = @event.Code,
                    LicenseeId = @event.LicenseeId,
                    LicenseeName = @event.LicenseeName,
                    TimeZoneId = @event.TimeZoneId
                });

                _repository.SaveChanges();
            }
        }

        public void Consume(BrandUpdated @event)
        {
            var brand = _repository.Brands.Single(x => x.Id == @event.Id);

            brand.Code = @event.Code;
            brand.Name = @event.Name;
            brand.LicenseeId = @event.LicenseeId;
            brand.LicenseeName = @event.LicenseeName;
            brand.TimeZoneId = @event.TimeZoneId;

            _repository.SaveChanges();
        }

        public void Consume(RiskLevelTagPlayer @event)
        {
            if (!_repository.PlayerRiskLevels.Any(x => x.Id == @event.Id))
            {
                var entity = new PlayerRiskLevel
                {
                    Id = @event.Id,
                    PlayerId = @event.PlayerId,
                    RiskLevelId = @event.RiskLevelId,
                    Description = @event.Description,
                    CreatedBy = @event.EventCreatedBy,
                    DateCreated = @event.EventCreated
                };


                _repository.PlayerRiskLevels.Add(entity);
                _repository.SaveChanges();
            }
        }

        public void Consume(RiskLevelUntagPlayer @event)
        {
            var entity = _repository.PlayerRiskLevels.SingleOrDefault(x => x.Id == @event.Id);
            if (entity != null)
            {
                _repository.PlayerRiskLevels.Remove(entity);
                _repository.SaveChanges();
            }
        }
    }
}
