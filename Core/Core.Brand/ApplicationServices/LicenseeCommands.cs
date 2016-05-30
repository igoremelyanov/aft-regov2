using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.ObjectBuilder2;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public interface ILicenseeCommands : IApplicationService
    {
        Guid Add(AddLicenseeData data);
        void Edit(EditLicenseeData data);
        void RenewContract(Guid licenseeId, string contractStart, string contractEnd);
        void Activate(Guid id, string remarks);
        void Deactivate(Guid id, string remarks);
    }

    public class LicenseeCommands : MarshalByRefObject, ILicenseeCommands
    {
        private readonly IBrandRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly LicenseeQueries _licenseeQueries;

        public LicenseeCommands(
            IBrandRepository repository, 
            IEventBus eventBus,
            IActorInfoProvider actorInfoProvider,
            LicenseeQueries licenseeQueries)
        {
            _repository = repository;
            _eventBus = eventBus;
            _actorInfoProvider = actorInfoProvider;
            _licenseeQueries = licenseeQueries;
        }

        [Permission(Permissions.Create, Module = Modules.LicenseeManager)]
        public Guid Add(AddLicenseeData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _licenseeQueries.ValidateCanAdd(data);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var licensee = new Licensee
                {
                    Id = data.Id ?? Guid.NewGuid(),
                    Name = data.Name,
                    CompanyName = data.CompanyName,
                    AffiliateSystem = data.AffiliateSystem,
                    ContractStart = data.ContractStart,
                    ContractEnd = data.ContractEnd,
                    Email = data.Email,
                    AllowedBrandCount = data.BrandCount,
                    AllowedWebsiteCount = data.WebsiteCount,
                    TimezoneId = data.TimeZoneId,
                    Status = LicenseeStatus.Inactive,
                    DateCreated = DateTimeOffset.UtcNow,
                    CreatedBy = _actorInfoProvider.Actor.UserName,
                    Contracts = new List<Contract>
                    {
                        new Contract
                        {
                            Id = Guid.NewGuid(),
                            StartDate = data.ContractStart,
                            EndDate = data.ContractEnd,
                            IsCurrentContract = true
                        }
                    }
                };

                if ( data.Products != null ) 
                    EnumerableExtensions.ForEach(data.Products, x => 
                        licensee.Products.Add(new LicenseeProduct
                        {
                            ProductId = new Guid(x)
                        }));

                EnumerableExtensions.ForEach(data.Currencies, x => licensee.Currencies.Add(_repository.Currencies.Single(y => y.Code == x)));
                EnumerableExtensions.ForEach(data.Countries, x => licensee.Countries.Add(_repository.Countries.Single(y => y.Code == x)));
                EnumerableExtensions.ForEach(data.Languages, x => licensee.Cultures.Add(_repository.Cultures.Single(y => y.Code == x)));

                _repository.Licensees.Add(licensee);
                _repository.SaveChanges();

                _eventBus.Publish(new LicenseeCreated
                {
                    Id = licensee.Id,
                    Name = licensee.Name,
                    CompanyName = licensee.CompanyName,
                    Email = licensee.Email,
                    AffiliateSystem = licensee.AffiliateSystem,
                    ContractStart = licensee.ContractStart,
                    ContractEnd = licensee.ContractEnd,
                    Languages = licensee.Cultures.Select(c => c.Code)
                });

                scope.Complete();

                return licensee.Id;
            }
        }

        [Permission(Permissions.Update, Module = Modules.LicenseeManager)]
        public void Edit(EditLicenseeData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _licenseeQueries.ValidateCanEdit(data);

                if (!validationResult.IsValid) 
                    throw new RegoValidationException(validationResult);

                var licensee = _repository.Licensees
                    .Include(x => x.Products)
                    .Include(x => x.Currencies)
                    .Include(x => x.Countries)
                    .Include(x => x.Cultures)
                    .Include(x => x.Contracts)
                    .Single(x => x.Id == data.Id);

                licensee.Name = data.Name;
                licensee.CompanyName = data.CompanyName;
                licensee.AffiliateSystem = data.AffiliateSystem;
                licensee.ContractStart = data.ContractStart;
                licensee.ContractEnd = data.ContractEnd;
                licensee.Email = data.Email;
                licensee.AllowedBrandCount = data.BrandCount;
                licensee.AllowedWebsiteCount = data.WebsiteCount;
                licensee.TimezoneId = data.TimeZoneId;
                licensee.DateUpdated = DateTimeOffset.UtcNow;
                licensee.UpdatedBy = _actorInfoProvider.Actor.UserName;
                licensee.Remarks = data.Remarks;

                var currentContract = licensee.Contracts.Single(x => x.IsCurrentContract);
                currentContract.StartDate = data.ContractStart;
                currentContract.EndDate = data.ContractEnd;

                licensee.Products.Clear();
                EnumerableExtensions.ForEach(data.Products, x =>
                    licensee.Products.Add(new LicenseeProduct
                    {
                        Licensee = licensee,
                        ProductId = new Guid(x)
                    }));

                licensee.Currencies.Clear();
                EnumerableExtensions.ForEach(data.Currencies, x => licensee.Currencies.Add(_repository.Currencies.Single(y => y.Code == x)));

                licensee.Countries.Clear();
                EnumerableExtensions.ForEach(data.Countries, x => licensee.Countries.Add(_repository.Countries.Single(y => y.Code == x)));

                licensee.Cultures.Clear();
                EnumerableExtensions.ForEach(data.Languages, x => licensee.Cultures.Add(_repository.Cultures.Single(y => y.Code == x)));

                _repository.SaveChanges();

                _eventBus.Publish(new LicenseeUpdated(licensee));

                scope.Complete();
            }
        }

        [Permission(Permissions.RenewContract, Module = Modules.LicenseeManager)]
        public void RenewContract(Guid licenseeId, string contractStart, string contractEnd)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var data = new RenewLicenseeContractData
                {
                    LicenseeId = licenseeId,
                    ContractStart = contractStart,
                    ContractEnd = contractEnd
                };

                var validationResult = new RenewLicenseeContractValidator(_repository).Validate(data);

                if (!validationResult.IsValid) 
                    throw new RegoValidationException(validationResult);

                var startDateTime = DateTimeOffset.Parse(contractStart);
                var endDateTime = !string.IsNullOrWhiteSpace(contractEnd)
                    ? DateTimeOffset.Parse(contractEnd)
                    : (DateTimeOffset?) null;

                var licensee = _repository.Licensees
                    .Include(x => x.Contracts)
                    .Single(x => x.Id == licenseeId);

                licensee.ContractStart = startDateTime;
                licensee.ContractEnd = endDateTime;
                licensee.Contracts.Single(x => x.IsCurrentContract).IsCurrentContract = false;

                licensee.Contracts.Add(new Contract
                {
                    Id = Guid.NewGuid(),
                    LicenseeId = licenseeId,
                    Licensee = licensee,
                    StartDate = startDateTime,
                    EndDate = endDateTime,
                    IsCurrentContract = true
                });

                licensee.DateUpdated = DateTimeOffset.UtcNow;
                licensee.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.LicenseeManager)]
        public void Activate(Guid id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var licensee = _repository.Licensees.SingleOrDefault(x => x.Id == id);

                var data = new ActivateLicenseeData
                {
                    Licensee = licensee,
                    Remarks = remarks
                };

                var validationResult = new ActivateLicenseeValidator().Validate(data);

                if (!validationResult.IsValid) 
                    throw new RegoValidationException(validationResult);

                licensee.Status = LicenseeStatus.Active;
                licensee.Remarks = remarks;
                licensee.ActivatedBy = _actorInfoProvider.Actor.UserName;
                licensee.DateActivated = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new LicenseeActivated
                {
                    Id = licensee.Id,
                    Remarks = licensee.Remarks
                });

                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.LicenseeManager)]
        public void Deactivate(Guid id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var licensee = _repository.Licensees.SingleOrDefault(x => x.Id == id);

                var data = new DeactivateLicenseeData
                {
                    Licensee = licensee,
                    Remarks = remarks
                };

                var validationResult = new DeactivateLicenseeValidator().Validate(data);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                licensee.Status = LicenseeStatus.Deactivated;
                licensee.Remarks = remarks;
                licensee.DeactivatedBy = _actorInfoProvider.Actor.UserName;
                licensee.DateDeactivated = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new LicenseeDeactivated
                {
                    Id = licensee.Id,
                    Remarks = licensee.Remarks
                });

                scope.Complete();
            }
        }
    }
}