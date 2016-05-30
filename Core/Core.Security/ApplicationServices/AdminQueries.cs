using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Security.Validators.User;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class AdminQueries : MarshalByRefObject, IAdminQueries
    {
        private readonly ISecurityRepository _repository;
        private readonly IAuthQueries _authQueries;
        private readonly IActorInfoProvider _actorInfoProvider;

        public AdminQueries(ISecurityRepository repository, IAuthQueries authQueries, IActorInfoProvider actorInfoProvider)
        {
            _repository = repository;
            _authQueries = authQueries;
            _actorInfoProvider = actorInfoProvider;
        }

        [Permission(Permissions.View, Module = Modules.AdminManager)]
        public IQueryable<Admin> GetAdmins()
        {
            return _repository.Admins.Include(u => u.Role).AsNoTracking();
        }

        public Admin GetAdminById(Guid adminId)
        {
            return _repository.Admins
                .Include(u => u.Role)
                .Include(u => u.Licensees)
                .Include(u => u.AllowedBrands)
                .Include(u => u.BrandFilterSelections)
                .Include(u => u.Currencies)
                .SingleOrDefault(u => u.Id == adminId);
        }

        public Admin GetAdminByName(string username)
        {
            return _repository.Admins
                .Include(u => u.Role)
                .Include(u => u.Licensees)
                .Include(u => u.AllowedBrands)
                .Include(u => u.Currencies)
                .SingleOrDefault(u => u.Username == username);
        }

        public ValidationResult GetValidationResult(LoginAdmin data)
        {
            var admin = _repository.Admins.SingleOrDefault(x => x.Username == data.Username);
            var adminValidationResult = new LoginValidator().Validate(admin);

            if (adminValidationResult.IsValid == false)
                return adminValidationResult;

            var loginValidationResult = _authQueries.GetValidationResult(new LoginActor
            {
                ActorId = admin.Id,
                Password = data.Password
            });

            return loginValidationResult;
        }

        public ValidationResult GetValidationResult(AddAdminData data)
        {
            var validator = new AddAdminValidator(_repository);
            return validator.Validate(data);
        }

        public ValidationResult GetValidationResult(EditAdminData data)
        {
            var validator = new EditAdminValidator();
            return validator.Validate(data);
        }

        public IEnumerable<Guid> GetLicenseeFilterSelections()
        {
            return _repository.Admins
                .Include(x => x.LicenseeFilterSelections)
                .Single(x => x.Id == _actorInfoProvider.Actor.Id)
                .LicenseeFilterSelections
                .Select(x => x.LicenseeId);
        }

        public IEnumerable<Guid> GetBrandFilterSelections()
        {
            return _repository.Admins
                .Include(x => x.BrandFilterSelections)
                .Single(x => x.Id == _actorInfoProvider.Actor.Id)
                .BrandFilterSelections
                .Select(x => x.BrandId);
        }

        public bool IsBrandAllowed(Guid adminId, Guid brandId)
        {
            var admin = _repository
                .Admins
                .Include(u => u.AllowedBrands)
                .SingleOrDefault(u => u.Id == adminId);

            return admin != null && admin.AllowedBrands.Any(brand => brand.Id == brandId);
        }
    }
}
