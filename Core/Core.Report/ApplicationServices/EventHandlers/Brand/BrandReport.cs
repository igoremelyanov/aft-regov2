using System;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class BrandReportEventHandlers
    {
        private readonly IUnityContainer _container;
        private const string BrandNotFoundMessage = "Unable to find BrandRecord with Id '{0}'";

        public BrandReportEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(BrandRegistered registeredEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.BrandRecords.SingleOrDefault(r => r.BrandId == registeredEvent.Id);
            if (record != null)
                throw new RegoException(string.Format("Brand with Id specified '{0}' already exists", registeredEvent.Id));

            record = new BrandRecord
            {
                BrandId = registeredEvent.Id,
                Licensee = registeredEvent.LicenseeName,
                BrandCode = registeredEvent.Code,
                Brand = registeredEvent.Name,
                BrandType = registeredEvent.BrandType.ToString(),
                BrandStatus = registeredEvent.Status.ToString(),
                BrandTimeZone = FormatTimeZoneOffset(registeredEvent.TimeZoneId),
                PlayerPrefix = registeredEvent.PlayerPrefix,
                AllowedInternalAccountsNumber = registeredEvent.InternalAccountsNumber,
                Created = registeredEvent.EventCreated,
                CreatedBy = registeredEvent.EventCreatedBy
            };
            if (registeredEvent.Status == BrandStatus.Active)
            {
                record.Activated = registeredEvent.EventCreated;
                record.ActivatedBy = registeredEvent.EventCreatedBy;
            }
            repository.BrandRecords.Add(record);
            repository.SaveChanges();
        }

        public void Handle(BrandUpdated updatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();

            //todo: refactor the following, updatedEvent.Id should not be the same as BrandId
            var record = repository.BrandRecords.SingleOrDefault(r => r.BrandId == updatedEvent.Id); 
            if (record == null)
                throw new RegoException(string.Format(BrandNotFoundMessage, updatedEvent.Id));
            
            record.Licensee = updatedEvent.LicenseeName;
            record.BrandCode = updatedEvent.Code;
            record.Brand = updatedEvent.Name;
            record.BrandType = updatedEvent.TypeName;
            record.Remarks = updatedEvent.Remarks;
            record.PlayerPrefix = updatedEvent.PlayerPrefix;
            record.BrandTimeZone = FormatTimeZoneOffset(updatedEvent.TimeZoneId);
            record.AllowedInternalAccountsNumber = updatedEvent.InternalAccountCount;
            record.Updated = updatedEvent.EventCreated;
            record.UpdatedBy = updatedEvent.EventCreatedBy;
            repository.SaveChanges();
        }

        public void Handle(BrandActivated activatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();

            //todo: refactor the following, updatedEvent.Id should not be the same as BrandId
            var record = repository.BrandRecords.SingleOrDefault(r => r.BrandId == activatedEvent.Id);
            if (record == null)
                throw new RegoException(string.Format(BrandNotFoundMessage, activatedEvent.Id));

            record.BrandStatus = BrandStatus.Active.ToString();
            record.Activated = activatedEvent.EventCreated;
            record.ActivatedBy = activatedEvent.EventCreatedBy;
            repository.SaveChanges();
        }

        public void Handle(BrandDeactivated deactivatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();

            //todo: refactor the following, updatedEvent.Id should not be the same as BrandId
            var record = repository.BrandRecords.SingleOrDefault(r => r.BrandId == deactivatedEvent.Id);
            if (record == null)
                throw new RegoException(string.Format(BrandNotFoundMessage, deactivatedEvent.Id));

            record.BrandStatus = BrandStatus.Deactivated.ToString();
            record.Deactivated = deactivatedEvent.EventCreated;
            record.DeactivatedBy = deactivatedEvent.EventCreatedBy;
            repository.SaveChanges();
        }

        private string FormatTimeZoneOffset(string timeZoneId)
        {
            return TimeZoneInfo.GetSystemTimeZones().Single(z => z.Id == timeZoneId).DisplayName;
        }
    }
}
