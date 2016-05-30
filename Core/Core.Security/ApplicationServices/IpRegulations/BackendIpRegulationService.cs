using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations
{
    public class BackendIpRegulationService : IpRegulationServiceBase
    {
        private const string DisableIpRegulationsSetting = "disable-ip-regulations";
        private readonly IEventBus _eventBus;

        private bool IpVerificationDisabled
        {
            get
            {
                var isIpRegulationDisabledDb =
                Repository.AdminIpRegulationSettings.FirstOrDefault(s => s.Name == DisableIpRegulationsSetting);

                var isIpRegulationDisabled = true;
                if (isIpRegulationDisabledDb == null) return isIpRegulationDisabled;

                if (!bool.TryParse(isIpRegulationDisabledDb.Value, out isIpRegulationDisabled))
                    throw new RegoException("Flag disable-ip-regulations set to incorrect value");

                return isIpRegulationDisabled;
            }

            set
            {
                var isIpRegulationDisabledDb =
                Repository.AdminIpRegulationSettings.FirstOrDefault(s => s.Name == DisableIpRegulationsSetting);

                if (isIpRegulationDisabledDb == null)
                {
                    isIpRegulationDisabledDb = new AdminIpRegulationSetting
                    {
                        Id = Guid.NewGuid(),
                        Name = DisableIpRegulationsSetting
                    };

                    Repository.AdminIpRegulationSettings.Add(isIpRegulationDisabledDb);
                }

                isIpRegulationDisabledDb.Value = value.ToString();

                Repository.SaveChanges();
            }
        }

        public BackendIpRegulationService(
            ISecurityRepository repository, 
            IActorInfoProvider data,
            IEventBus eventBus
            )
            :base(repository, data)
        {
            _eventBus = eventBus;
        }

        [Permission(Permissions.Update, Module = Modules.BackendIpRegulationManager)]
        public void SetIpVerificationDisabled(bool targetState)
        {
            IpVerificationDisabled = targetState;
        }

        [Permission(Permissions.View, Module = Modules.BackendIpRegulationManager)]
        public IQueryable<AdminIpRegulation> GetIpRegulations()
        {
            return Repository.AdminIpRegulations.Include(ip => ip.CreatedBy).Include(ip => ip.UpdatedBy).AsQueryable();
        }

        public AdminIpRegulation GetIpRegulation(Guid id)
        {
            return GetIpRegulations().SingleOrDefault(ip => ip.Id == id);
        }

        [Permission(Permissions.Create, Module = Modules.BackendIpRegulationManager)]
        public AdminIpRegulation CreateIpRegulation(AddBackendIpRegulationData data)
        {
            var regulation = Mapper.DynamicMap<AdminIpRegulation>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.CreatedBy = Repository.Admins.SingleOrDefault(u => u.Id == ActorInfoProvider.Actor.Id);
                regulation.CreatedDate = DateTime.Now;
                regulation.Id = Guid.NewGuid();
                Repository.AdminIpRegulations.Add(regulation);
                Repository.SaveChanges();

                _eventBus.Publish(new AdminIpRegulationCreated(regulation));
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Update, Module = Modules.BackendIpRegulationManager)]
        public AdminIpRegulation UpdateIpRegulation(EditBackendIpRegulationData data)
        {
            var regulation = Repository.AdminIpRegulations.SingleOrDefault(ip => ip.Id == data.Id);

            if (regulation == null)
            {
                throw new RegoException("User does not exist");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.IpAddress = data.IpAddress;
                regulation.Description = data.Description;

                regulation.UpdatedBy = Repository.Admins.SingleOrDefault(u => u.Id == ActorInfoProvider.Actor.Id);
                regulation.UpdatedDate = DateTime.Now;

                Repository.SaveChanges();

                _eventBus.Publish(new AdminIpRegulationUpdated(regulation));
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Delete, Module = Modules.BackendIpRegulationManager)]
        public void DeleteIpRegulation(Guid id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var ipRegulation = GetIpRegulation(id);
                Repository.AdminIpRegulations.Remove(ipRegulation);

                Repository.SaveChanges();

                _eventBus.Publish(new AdminIpRegulationDeleted(ipRegulation));
                scope.Complete();
            }
        }

        public bool IsIpAddressUnique(string address)
        {
            return !IsIpAddressValid(address) || !Repository
                .AdminIpRegulations.ToList()
                .Any(ip => IsRangesIntersects(ip.IpAddress, address));
        }

        public bool VerifyIpAddress(string ipAddress)
        {
            if (IpVerificationDisabled || IsLocalhost(ipAddress))
            {
                return true;
            }

            if (!IsIpAddressValid(ipAddress))
            {
                return false;
            }

            var ipRegulation =
                    Repository.AdminIpRegulations.ToList().FirstOrDefault(
                    ip =>
                        IsRangesIntersects(ip.IpAddress, ipAddress));

            return  ipRegulation != null;
        }
    }
}
