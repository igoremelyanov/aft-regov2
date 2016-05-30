using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Data;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;

namespace AFT.RegoV2.Core.Auth.ApplicationServices
{
    public class AuthCommands : IAuthCommands
    {
        private readonly IAuthRepository _repository;
        private readonly IAuthQueries _authQueries;

        public AuthCommands(IAuthRepository repository, IAuthQueries authQueries)
        {
            _repository = repository;
            _authQueries = authQueries;
        }

        public void CreateActor(CreateActor model)
        {
            var result = _authQueries.GetValidationResult(model);
            if (result.IsValid == false)
                throw new ApplicationException(result.Errors.First().ErrorMessage);

            var actor = new Entities.Actor(model.ActorId, model.Username, model.Password);
            _repository.Actors.Add(actor.Data);
            _repository.SaveChanges();
        }

        public void ChangePassword(ChangePassword model)
        {
            var result = _authQueries.GetValidationResult(model);
            if (result.IsValid == false)
                throw new ApplicationException(result.Errors.First().ErrorMessage);

            var actor = _repository.GetActor(model.ActorId);
            actor.ChangePassword(model.NewPassword);
            _repository.SaveChanges();
        }

        public void CreateRole(CreateRole model)
        {
            var result = _authQueries.GetValidationResult(model);
            if (result.IsValid == false)
                throw new ApplicationException(result.Errors.First().ErrorMessage);

            var role = new Role
            {
                Id = model.RoleId,
                Permissions = _repository.Permissions.Where(p => model.Permissions.Contains(p.Id)).ToList()
            };
            _repository.Roles.Add(role);
            _repository.SaveChanges();
        }

        public void UpdateRole(UpdateRole model)
        {
            var result = _authQueries.GetValidationResult(model);
            if (result.IsValid == false)
                throw new ApplicationException(result.Errors.First().ErrorMessage);

            var role = _repository
                .Roles
                .Include(r => r.Permissions)
                .Single(r => r.Id == model.RoleId);

            role.Permissions.Clear();
            role.Permissions = _repository.Permissions.Where(p => model.Permissions.Contains(p.Id)).ToList();
            _repository.SaveChanges();
        }

        public void CreatePermission(CreatePermission model)
        {
            var result = _authQueries.GetValidationResult(model);
            if (result.IsValid == false)
                throw new ApplicationException(result.Errors.First().ErrorMessage);

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Module = model.Module
            };
            _repository.Permissions.Add(permission);
            _repository.SaveChanges();
        }

        public void AssignRoleToActor(AssignRole model)
        {
            var result = _authQueries.GetValidationResult(model);
            if (result.IsValid == false)
                throw new ApplicationException(result.Errors.First().ErrorMessage);

            var actor = _repository.GetActor(model.ActorId);
            var role = _repository.Roles.Single(r => r.Id == model.RoleId);
            actor.AssignRole(role);
            _repository.SaveChanges();
        }
    }
}