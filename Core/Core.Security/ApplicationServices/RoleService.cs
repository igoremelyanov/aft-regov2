using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class RoleService : MarshalByRefObject, IApplicationService
    {
        private readonly ISecurityRepository _repository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;
        private readonly IAuthCommands _authCommands;

        public RoleService(
            ISecurityRepository repository,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus,
            IAuthCommands authCommands)
        {
            _repository = repository;
            _eventBus = eventBus;
            _authCommands = authCommands;
            _actorInfoProvider = actorInfoProvider;
        }

        public Role GetRoleById(Guid roleId)
        {
            return _repository.Roles
                .Include(r => r.CreatedBy)
                .Include(r => r.UpdatedBy)
                .Include(r => r.Licensees)
                .SingleOrDefault(r => r.Id == roleId);
        }

        [Permission(Permissions.View, Module = Modules.RoleManager)]
        [Permission(Permissions.View, Module = Modules.AdminManager)]
        public IQueryable<Role> GetRoles()
        {
            return _repository.Roles.Include(x => x.CreatedBy)
                .Include(r => r.UpdatedBy).Include(r => r.Licensees)
                .Include(r => r.Licensees)
                .AsQueryable();
        }

        [Permission(Permissions.Create, Module = Modules.RoleManager)]
        public void CreateRole(AddRoleData data)
        {
            var role = Mapper.DynamicMap<Role>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                role.Id = Guid.NewGuid();
                role.CreatedBy = _repository.Admins.SingleOrDefault(u => u.Id == _actorInfoProvider.Actor.Id);
                role.CreatedDate = DateTimeOffset.UtcNow;

                _authCommands.CreateRole(new CreateRole
                {
                    RoleId = role.Id,
                    Permissions = data.CheckedPermissions
                });

                role.SetLicensees(data.AssignedLicensees);

                _repository.Roles.Add(role);

                _repository.SaveChanges();

                _eventBus.Publish(new RoleCreated(role));

                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Module = Modules.RoleManager)]
        public void UpdateRole(EditRoleData data)
        {
            var role = GetRoleById(data.Id);
            if (role == null)
                throw new RegoException("Role not found");

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                role.Code = data.Code;
                role.Name = data.Name;
                role.Description = data.Description;
                role.UpdatedBy = _repository.Admins.Single(u => u.Id == _actorInfoProvider.Actor.Id);
                role.UpdatedDate = DateTimeOffset.UtcNow;

                _authCommands.UpdateRole(new UpdateRole
                {
                    RoleId = role.Id,
                    Permissions = data.CheckedPermissions
                });

                role.SetLicensees(data.AssignedLicensees);

                _repository.SaveChanges();

                _eventBus.Publish(new RoleUpdated(role));

                scope.Complete();
            }
        }
    }
}
