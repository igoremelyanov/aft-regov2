using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Security.Validators.IpRegulations;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using FluentValidation.Results;
using EditBrandIpRegulationData = AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations.EditBrandIpRegulationData;

namespace AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations
{
    public class BrandIpRegulationService : IpRegulationServiceBase
    {
        private readonly IEventBus _eventBus;

        public BrandIpRegulationService(
            ISecurityRepository repository, 
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus
            )
            : base(repository, actorInfoProvider)
        {
            _eventBus = eventBus;
        }

        #region Queries

        [Permission(Permissions.View, Module = Modules.BrandIpRegulationManager)]
        public IQueryable<BrandIpRegulation> GetIpRegulations()
        {
            return Repository.BrandIpRegulations.Include(ip => ip.CreatedBy).Include(ip => ip.UpdatedBy).AsQueryable();
        }

        public BrandIpRegulation GetIpRegulation(Guid id)
        {
            return GetIpRegulations().SingleOrDefault(ip => ip.Id == id);
        }

        #endregion

        [Permission(Permissions.Create, Module = Modules.BrandIpRegulationManager)]
        public BrandIpRegulation CreateIpRegulation(AddBrandIpRegulationData data)
        {
            var regulation = Mapper.DynamicMap<BrandIpRegulation>(data);
            var brand = Repository.Brands.Single(x => x.Id == data.BrandId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.Id = Guid.NewGuid();
                regulation.CreatedBy = Repository.Admins.SingleOrDefault(u => u.Id == ActorInfoProvider.Actor.Id);
                regulation.CreatedDate = DateTimeOffset.Now.ToBrandOffset(brand.TimeZoneId);

                Repository.BrandIpRegulations.Add(regulation);
                Repository.SaveChanges();

                _eventBus.Publish(new BrandIpRegulationCreated(regulation)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimeZoneId),
                });

                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Update, Module = Modules.BrandIpRegulationManager)]
        public BrandIpRegulation UpdateIpRegulation(EditBrandIpRegulationData data)
        {
            var regulation = Repository.BrandIpRegulations.SingleOrDefault(ip => ip.Id == data.Id);
            var brand = Repository.Brands.Single(x => x.Id == data.BrandId);

            if (regulation == null)
            {
                throw new RegoException("User does not exist");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.LicenseeId = data.LicenseeId;
                regulation.BrandId = data.BrandId;
                regulation.IpAddress = data.IpAddress;
                regulation.Description = data.Description;
                regulation.BlockingType = data.BlockingType;
                regulation.RedirectionUrl = data.RedirectionUrl;

                regulation.UpdatedBy = Repository.Admins.SingleOrDefault(u => u.Id == ActorInfoProvider.Actor.Id);
                regulation.UpdatedDate = DateTimeOffset.Now.ToBrandOffset(brand.TimeZoneId);

                Repository.SaveChanges();

                _eventBus.Publish(new BrandIpRegulationUpdated(regulation)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimeZoneId),
                });
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Delete, Module = Modules.BrandIpRegulationManager)]
        public void DeleteIpRegulation(BrandIpRegulationId id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var ipRegulation = GetIpRegulation(id);

                var brand = Repository.Brands.Single(x => x.Id == ipRegulation.BrandId);

                Repository.BrandIpRegulations.Remove(ipRegulation);

                Repository.SaveChanges();

                _eventBus.Publish(new BrandIpRegulationDeleted(ipRegulation)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(brand.TimeZoneId),
                });
                scope.Complete();
            }
        }

        public bool IsIpAddressUnique(string address)
        {
            return !IsIpAddressValid(address) || !Repository
                .BrandIpRegulations.ToList()
                .Any(ip => IsRangesIntersects(ip.IpAddress, address));
        }

        public VerifyIpResult VerifyIpAddress(string ipAddress, Guid? brandId = null)
        {
            var result = new VerifyIpResult();

            if (IsLocalhost(ipAddress))
            {
                result.Allowed = true;
                return result;
            }

            if (!IsIpAddressValid(ipAddress))
            {
                result.Allowed = false;
                return result;
            }

            var ipRegulation =
                    Repository.BrandIpRegulations.ToList().FirstOrDefault(
                    ip =>
                        IsRangesIntersects(ip.IpAddress, ipAddress));

            result.Allowed = !(ipRegulation != null && (!brandId.HasValue || ipRegulation.BrandId == brandId.Value));

            if (result.Allowed) return result;

            result.BlockingType = ipRegulation.BlockingType;
            result.RedirectionUrl = ipRegulation.RedirectionUrl;

            return result;
        }

        public ValidationResult ValidateAddBrandData(AddBrandIpRegulationData data)
        {
            var validator = new AddBrandIpRegulationValidator();
            return validator.Validate(data);
        }
    }
}
