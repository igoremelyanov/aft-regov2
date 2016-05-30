using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Security.Validators.User;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class AdminCommands : MarshalByRefObject, IAdminCommands
    {
        private readonly ISecurityRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IAuthCommands _authCommands;
        private readonly IActorInfoProvider _actorInfoProvider;

        public AdminCommands(
            ISecurityRepository repository,
            IEventBus eventBus,
            IAuthCommands authCommands,
            IActorInfoProvider actorInfoProvider
            )
        {
            _repository = repository;
            _eventBus = eventBus;
            _authCommands = authCommands;
            _actorInfoProvider = actorInfoProvider;
        }

        [Permission(Permissions.Create, Module = Modules.AdminManager)]
        public Admin CreateAdmin(AddAdminData data)
        {
            var validationResult = new AddAdminValidator(_repository).Validate(data);

            if (!validationResult.IsValid)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            //todo: KB: not sure how input role id can be null. And if it is validation should trigger
            var role = _repository.Roles.SingleOrDefault(r => r.Id == (data.RoleId ?? new Guid("00000000-0000-0000-0000-000000000002")));

            var admin = Mapper.DynamicMap<Admin>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                admin.Id = Guid.NewGuid();

                admin.Role = role;

                admin.SetLicensees(data.AssignedLicensees);

                admin.SetAllowedBrands(data.AllowedBrands);

                admin.SetCurrencies(data.Currencies);

                if (data.AllowedBrands != null)
                {
                    foreach (var allowedBrand in data.AllowedBrands)
                    {
                        admin.BrandFilterSelections.Add(new BrandFilterSelection
                        {
                            AdminId = admin.Id,
                            BrandId = allowedBrand,
                            Admin = admin
                        });
                    }
                }

                _authCommands.CreateActor(new CreateActor
                {
                    ActorId = admin.Id,
                    Username = admin.Username,
                    Password = data.Password
                });
                _authCommands.AssignRoleToActor(new AssignRole
                {
                    ActorId = admin.Id,
                    RoleId = role.Id
                });

                _repository.Admins.Add(admin);
                _repository.SaveChanges();

                _eventBus.Publish(new AdminCreated(admin));

                scope.Complete();
            }

            return admin;
        }

        [Permission(Permissions.Update, Module = Modules.AdminManager)]
        public void UpdateAdmin(EditAdminData data)
        {
            var validationResult = new EditAdminValidator().Validate(data);

            if (!validationResult.IsValid)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            var admin = _repository.GetAdminById(data.Id);

            //todo: KB: should be handled by EditAdminValidator
            if (admin == null)
            {
                throw new RegoException(string.Format("User with id: {0} not found", data.Id));
            }

            var role = _repository.Roles.SingleOrDefault(r => r.Id == data.RoleId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                admin.Username = data.Username;
                admin.FirstName = data.FirstName;
                admin.LastName = data.LastName;
                admin.IsActive = data.IsActive;
                admin.Language = data.Language;
                admin.Description = data.Description;

                //todo: KB: should be handled by EditAdminValidator
                if (role != null)
                {
                    admin.Role = role;
                }

                admin.SetLicensees(data.AssignedLicensees);

                admin.SetAllowedBrands(data.AllowedBrands);

                admin.SetCurrencies(data.Currencies);

                if(data.RoleId != role.Id)
                    _authCommands.AssignRoleToActor(new AssignRole
                    {
                        ActorId = admin.Id,
                        RoleId = role.Id
                    });

                _repository.SaveChanges();

                _eventBus.Publish(new AdminUpdated(admin));

                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Module = Modules.AdminManager)]
        public void ChangePassword(AdminId adminId, string password)
        {
            _authCommands.ChangePassword(new ChangePassword
            {
                ActorId = adminId,
                NewPassword = password
            });
            _eventBus.Publish(new AdminPasswordChanged(adminId));
        }

        [Permission(Permissions.Activate, Module = Modules.AdminManager)]
        public void Activate(ActivateUserData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ChangeStatus(data.Id, true);
                _eventBus.Publish(new AdminActivated(data.Id, data.Remarks));
                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.AdminManager)]
        public void Deactivate(DeactivateUserData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ChangeStatus(data.Id, false);
                _eventBus.Publish(new AdminDeactivated(data.Id, data.Remarks));
                scope.Complete();
            }
        }

        private void ChangeStatus(Guid adminId, bool isActive)
        {
            var user = _repository.Admins.SingleOrDefault(u => u.Id == adminId);

            if (user == null)
                throw new RegoException(string.Format("User with id: {0} not found", adminId));

            user.IsActive = isActive;

            _repository.SaveChanges();
        }

        public void AddBrandToAdmin(Guid adminId, Guid brandId)
        {
            var user = _repository.GetAdminById(adminId);

            if (user == null)
            {
                throw new SecurityException(string.Format("User with id: {0} not found", adminId));
            }

            user.AddAllowedBrand(brandId);
            _repository.SaveChanges();
        }

        public void RemoveBrandFromAdmin(Guid adminId, Guid brandId)
        {
            var user = _repository.GetAdminById(adminId);

            if (user == null)
            {
                throw new SecurityException(string.Format("User with id: {0} not found", adminId));
            }

            user.RemoveAllowedBrand(brandId);
            _repository.SaveChanges();
        }

        public void SetLicenseeFilterSelections(IEnumerable<Guid> selectedLicensees)
        {
            var user = _repository.Admins
                .Include(x => x.LicenseeFilterSelections)
                .Single(x => x.Id == _actorInfoProvider.Actor.Id);

            user.LicenseeFilterSelections.Clear();

            if (selectedLicensees != null)
            {
                selectedLicensees.ForEach(x => user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    AdminId = user.Id,
                    LicenseeId = x,
                    Admin = user
                }));
            }

            _repository.SaveChanges();
        }

        public void SetBrandFilterSelections(IEnumerable<Guid> selectedBrands)
        {
            var user = _repository.Admins
                .Include(x => x.BrandFilterSelections)
                .Single(x => x.Id == _actorInfoProvider.Actor.Id);

            user.BrandFilterSelections.Clear();

            if (selectedBrands != null)
            {
                selectedBrands.ForEach(x => user.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    AdminId = user.Id,
                    BrandId = x,
                    Admin = user
                }));
            }

            _repository.SaveChanges();
        }
    }
}
