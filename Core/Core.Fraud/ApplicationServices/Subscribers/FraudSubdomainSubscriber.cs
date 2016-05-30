using System;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Services;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;
using PaymentLevelStatus = AFT.RegoV2.Core.Payment.Interface.Data.PaymentLevelStatus;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class FraudSubdomainSubscriber :
        IConsumes<PaymentLevelAdded>,
        IConsumes<PaymentLevelActivated>,
        IConsumes<PaymentLevelDeactivated>,
        IConsumes<PaymentLevelEdited>,
        IConsumes<PlayerUpdated>,
        IConsumes<PlayerRegistrationChecked>,
        IConsumes<VipLevelActivated>,
        IConsumes<VipLevelDeactivated>,
        IConsumes<VipLevelRegistered>
    {
        private readonly IUnityContainer _container;

        public FraudSubdomainSubscriber(
            IUnityContainer container)
        {
            _container = container;
        }

        public void Consume(PaymentLevelAdded @event)
        {
            var repository = _container.Resolve<IFraudRepository>();

            try
            {
                var newPaymentLevel = new PaymentLevel
                {
                    Code = @event.Code,
                    Name = @event.Name,
                    BrandId = @event.BrandId,
                    CurrencyCode = @event.CurrencyCode,
                    Id = @event.Id,
                    Status = (Interface.Data.PaymentLevelStatus)@event.Status
                };

                repository.PaymentLevels.AddOrUpdate(newPaymentLevel);
                repository.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                throw new RegoException(e.Message);
            }
        }

        public void Consume(PaymentLevelActivated @event)
        {
            var repository = _container.Resolve<IFraudRepository>();

            try
            {
                var recordToBeUpdated = repository
                    .PaymentLevels.SingleOrDefault(rec => rec.Id == @event.Id);

                if (recordToBeUpdated != null)
                {
                    recordToBeUpdated.Status = Interface.Data.PaymentLevelStatus.Active;
                    repository.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new RegoException(e.Message);
            }
        }

        public void Consume(PaymentLevelDeactivated @event)
        {
            var repository = _container.Resolve<IFraudRepository>();

            //There msut be a logic here that changes the values of the columns DeactivatedBy, DateDeactivated(For now the columns are missing)
            try
            {
                var recordToBeUpdated = repository
                    .PaymentLevels.SingleOrDefault(rec => rec.Id == @event.Id);

                if (recordToBeUpdated != null)
                {
                    recordToBeUpdated.Status = (Interface.Data.PaymentLevelStatus)PaymentLevelStatus.Inactive;
                    repository.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new RegoException(e.Message);
            }
        }

        public void Consume(PaymentLevelEdited @event)
        {
            var repository = _container.Resolve<IFraudRepository>();

            //There must be a logic that changes the values of the record with id: @event.Id
            try
            {
                var updatedPaymentLevel = new PaymentLevel()
                {
                    Name = @event.Name,
                    Code = @event.Code,
                    Status = (Interface.Data.PaymentLevelStatus)@event.Status,
                    CurrencyCode = @event.CurrencyCode,
                    BrandId = @event.BrandId,
                    Id = @event.Id
                };

                repository.PaymentLevels.Attach(updatedPaymentLevel);
                repository.SaveChanges();
            }
            catch (Exception e)
            {
                throw new RegoException(e.Message);
            }
        }

        public void Consume(PlayerRegistered @event)
        {
            var repository = _container.Resolve<IFraudRepository>();
            
            var player = new Fraud.Data.Player
            {
                DateRegistered = @event.DateRegistered,
                Email = @event.Email,
                FirstName = @event.FirstName,
                Id = @event.PlayerId,
                BrandId = @event.BrandId,
                IPAddress = @event.IPAddress,
                LastName = @event.LastName,
                Phone = @event.PhoneNumber,
                Username = @event.UserName,
                ZipCode = @event.ZipCode,
                Address = @event.AddressLines.Aggregate((x, y) => x + y)
            };

            try
            {
                repository.Players.Add(player);
                repository.SaveChanges();

                var duplicationMatchingService = _container.Resolve<IDuplicationMatchingService>();
                var duplicationService = _container.Resolve<IDuplicationService>();

                //Todo: sould replace it with async task. 
                duplicationMatchingService.Match(player.Id);
                duplicationService.ApplyAction(player.Id);
            }
            catch (Exception e)
            {
                throw new RegoException(e.Message, e);
            }
        }

        public void Consume(PlayerUpdated message)
        {
            //todo: talk to BA about updating.
        }

        public void Consume(VipLevelActivated message)
        {
            var repository = _container.Resolve<IFraudRepository>();

            var vipLevel = repository.VipLevels
                .Single(o => o.Id == message.VipLevelId);

            vipLevel.Status = VipLevelStatus.Active;

            repository.VipLevels.AddOrUpdate(vipLevel);
            repository.SaveChanges();
        }

        public void Consume(VipLevelDeactivated message)
        {
            var repository = _container.Resolve<IFraudRepository>();

            var vipLevel = repository.VipLevels
                .Single(o => o.Id == message.VipLevelId);

            vipLevel.Status = VipLevelStatus.Inactive;

            repository.VipLevels.AddOrUpdate(vipLevel);
            repository.SaveChanges();
        }

        public void Consume(VipLevelRegistered message)
        {
            var repository = _container.Resolve<IFraudRepository>();

            repository.VipLevels.Add(new VipLevel
            {
                Id = message.Id,
                BrandId = message.BrandId,
                Code = message.Code,
                Description = message.Description,
                Name = message.Name,
                Status = message.Status
            });

            repository.SaveChanges();
        }

        public void Consume(PlayerRegistrationChecked message)
        {
            var repository = _container.Resolve<IFraudRepository>();
            var player = repository.Players.SingleOrDefault(x => x.Id == message.PlayerId);
            player.Tag = message.Tag;

            repository.SaveChanges();
        }
    }
}
