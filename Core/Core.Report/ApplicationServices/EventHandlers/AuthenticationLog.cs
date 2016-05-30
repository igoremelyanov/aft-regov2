using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Security.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class AuthenticationLogEventHandlers
    {
        private readonly IUnityContainer _container;

        public AuthenticationLogEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(AdminAuthenticationSucceded @event)
        {
            var repository = _container.Resolve<IReportRepository>();
            repository.AdminAuthenticationLog.Add(new AdminAuthenticationLog
            {
                Id = Identifier.NewSequentialGuid(),
                PerformedBy = @event.EventCreatedBy,
                DatePerformed = @event.EventCreated,
                IPAddress = @event.IPAddress,
                Headers = string.Join("\n", @event.Headers.Select(h => string.Format("{0}: {1}", h.Key, h.Value)))
            });
            repository.SaveChanges();
        }

        public void Handle(AdminAuthenticationFailed @event)
        {
            var repository = _container.Resolve<IReportRepository>();
            repository.AdminAuthenticationLog.Add(new AdminAuthenticationLog
            {
                Id = Identifier.NewSequentialGuid(),
                PerformedBy = @event.EventCreatedBy,
                DatePerformed = @event.EventCreated,
                IPAddress = @event.IPAddress,
                Headers = string.Join("\n", @event.Headers.Select(h => string.Format("{0}: {1}", h.Key, h.Value))),
                FailReason = @event.FailReason
            });
            repository.SaveChanges();
        }

        public void Handle(MemberAuthenticationSucceded @event)
        {
            var repository = _container.Resolve<IReportRepository>();
            var brandQueries = _container.Resolve<BrandQueries>();
            var brand = brandQueries.GetBrand(@event.BrandId);
            var logEntry = new MemberAuthenticationLog
            {
                Id = Identifier.NewSequentialGuid(),
                Brand = brand.Code,
                BrandId = brand.Id,
                PerformedBy = @event.EventCreatedBy,
                DatePerformed = @event.EventCreated,
                IPAddress = @event.IPAddress
            };
            if (@event.Headers != null)
                logEntry.Headers = string.Join("\n", @event.Headers.Select(h => string.Format("{0}: {1}", h.Key, h.Value)));

            repository.MemberAuthenticationLog.Add(logEntry);
            repository.SaveChanges();
        }

        public void Handle(MemberAuthenticationFailed @event)
        {
            var repository = _container.Resolve<IReportRepository>();
            var brandQueries = _container.Resolve<BrandQueries>();
            var brand = brandQueries.GetBrand(@event.BrandId);
            var logEntry = new MemberAuthenticationLog
            {
                Id = Identifier.NewSequentialGuid(),
                Brand = brand.Code,
                BrandId = brand.Id,
                PerformedBy = @event.Username,
                DatePerformed = @event.EventCreated,
                IPAddress = @event.IPAddress,
                FailReason = @event.FailReason
            };
            if (@event.Headers != null)
                logEntry.Headers = string.Join("\n", @event.Headers.Select(h => string.Format("{0}: {1}", h.Key, h.Value)));

            repository.MemberAuthenticationLog.Add(logEntry);
            repository.SaveChanges();
        }
    }
}
