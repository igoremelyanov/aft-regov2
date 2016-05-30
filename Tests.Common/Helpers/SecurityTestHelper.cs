using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using Brand = AFT.RegoV2.Core.Brand.Interface.Data.Brand;
using Role = AFT.RegoV2.Core.Security.Data.Users.Role;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class SecurityTestHelper
    {
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly ISecurityRepository _securityRepository;
        private readonly RoleService _roleService;
        private readonly BrandQueries _brandQueries;
        private readonly IAuthQueries _authQueries;
        private readonly ApplicationSeeder _applicationSeeder;
        private readonly IAuthCommands _authCommands;
        private readonly IAuthRepository _authRepository;
        private readonly IAdminCommands _adminCommands;
        private readonly ClaimsIdentityProvider _identityProvider;
        private readonly BrandIpRegulationService _brandIpRegulationService;


        public SecurityTestHelper(
            IActorInfoProvider actorInfoProvider,
            ISecurityRepository securityRepository,
            RoleService roleService,
            BrandQueries brandQueries,
            IAuthQueries authQueries,
            ApplicationSeeder applicationSeeder,
            IAuthCommands authCommands,
            IAuthRepository authRepository,
            IAdminCommands adminCommands, 
            ClaimsIdentityProvider identityProvider, BrandIpRegulationService brandIpRegulationService)
        {
            _actorInfoProvider = actorInfoProvider;
            _securityRepository = securityRepository;
            _roleService = roleService;
            _brandQueries = brandQueries;
            _authQueries = authQueries;
            _applicationSeeder = applicationSeeder;
            _authCommands = authCommands;
            _authRepository = authRepository;
            _adminCommands = adminCommands;
            _identityProvider = identityProvider;
            _brandIpRegulationService = brandIpRegulationService;
        }

        public void CreateAndSignInSuperAdmin()
        {
            var admin = CreateSuperAdmin();

            SignInAdmin(admin);
        }

        public Admin CreateSuperAdmin()
        {
            var adminId = RoleIds.SuperAdminId;
            var roleId = RoleIds.SuperAdminId;
            var superAdmin = "SuperAdmin";

            var role = new Role
            {
                Id = roleId,
                Code = superAdmin,
                Name = superAdmin,
                CreatedDate = DateTime.UtcNow
            };

            var admin = new Admin
            {
                Id = adminId,
                Username = superAdmin,
                FirstName = superAdmin,
                LastName = superAdmin,
                IsActive = true,
                Description = superAdmin,
                Role = role
            };

            _securityRepository.Admins.AddOrUpdate(admin);
            _securityRepository.SaveChanges();

            _authCommands.CreateRole(new CreateRole
            {
                RoleId = roleId,
                Permissions = _authRepository.Permissions.Select(p => p.Id).ToList()
            });
            _authCommands.CreateActor(new CreateActor
            {
                ActorId = admin.Id,
                Username = admin.Username,
                Password = admin.Username
            });
            _authCommands.AssignRoleToActor(new AssignRole
            {
                ActorId = adminId,
                RoleId = role.Id
            });

            return admin;
        }

        public void SignInAdmin(Admin admin)
        {
            _actorInfoProvider.Actor.UserName = admin.Username;
            _actorInfoProvider.Actor.Id = admin.Id;
        }

        /// <summary>
        /// Use in selenium/integration tests
        /// </summary>
        public void SignInClaimsSuperAdmin()
        {
            var identity = _identityProvider.GetActorIdentity(RoleIds.SuperAdminId, "Tests");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
        }

        public void PopulatePermissions()
        {
            _applicationSeeder.PopulatePermissions();
        }

        public bool IsRoleExistInDb(Guid roleId)
        {
            return _roleService.GetRoleById(roleId) != null;
        }

        public Role CreateRole(Guid[] licenseeIds = null, ICollection<PermissionData> permissions = null)
        {
            var addRoleData = new AddRoleData
            {
                Code = "Role-" + TestDataGenerator.GetRandomString(5),
                Name = "Role-" + TestDataGenerator.GetRandomString(5),
                Description = TestDataGenerator.GetRandomString(),
                CheckedPermissions = (permissions != null ? permissions.Select(p => p.Id)
                    : _authQueries.GetPermissions().Select(o => o.Id)).ToList(),
                AssignedLicensees = licenseeIds
            };

            _roleService.CreateRole(addRoleData);

            return _roleService.GetRoles().Single(r => r.Name == addRoleData.Name);
        }

        public BrandIpRegulation CreateBrandIpRegulation(string ipAddress, string brandName,
            string redirectUrl = null, string blockingType = null)
        {
            var brand = _brandQueries.GetBrands().SingleOrDefault(b => b.Name == brandName);

            if (brand == null)
                throw new RegoValidationException(SecurityErrorCodes.InvalidBrandCode.ToString());

            var addIpRegulationData = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                LicenseeId = brand.LicenseeId,
                BrandId = brand.Id,
                RedirectionUrl = redirectUrl,
                BlockingType = blockingType
            };
            return _brandIpRegulationService.CreateIpRegulation(addIpRegulationData);
        }

        public Admin CreateAdmin(Guid licenseeId, IEnumerable<Brand> brands = null, IEnumerable<string> currencies = null, string password = null, Guid? roleId = null, bool isActive = true)
        {
            var licenseeIds = new[] { licenseeId };

            return CreateAdmin(licenseeIds, brands, currencies, password, roleId, isActive);
        }

        public Admin CreateAdmin(string category, string[] permissions, IEnumerable<Brand> brands = null, string password = null)
        {
            // Create role and user that have provided permissions
             var insufficientPermissions = permissions.Select(
                insuffPermission => _authQueries.GetPermissions().First(p => p.Name == insuffPermission && p.Module == category)).ToList();

            var roleWithoutPermission = CreateRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateAdmin(Guid.NewGuid(), brands, password: password, roleId: roleWithoutPermission.Id);

            return userWithoutPermission;
        }

        public Admin CreateAdmin(IEnumerable<Guid> licenseeIds = null, IEnumerable<Brand> brands = null, IEnumerable<string> currencies = null, string password = null, Guid? roleId = null, bool isActive = true)
        {
            var userName = "User-" + TestDataGenerator.GetRandomString(5);

            if (password == null)
                password = TestDataGenerator.GetRandomString();

            licenseeIds = licenseeIds ?? _brandQueries.GetLicensees().Select(l => l.Id);

            if (roleId == null)
            {
                var role = CreateRole(licenseeIds.ToArray());
                roleId = role.Id;
            }

            brands = brands ?? _brandQueries.GetBrands();
            currencies = currencies ?? _brandQueries.GetCurrencies().Select(c => c.Code);

            var userData = new AddAdminData
            {
                Username = userName,
                FirstName = userName,
                LastName = userName,
                Password = password,
                Language = "English",
                IsActive = isActive,
                AssignedLicensees = licenseeIds.ToList(),
                AllowedBrands = brands.Select(b => b.Id).ToList(),
                Currencies = currencies.ToList(),
                RoleId = roleId
            };

            return _adminCommands.CreateAdmin(userData);
        }

        public PermissionData GetPermission(string permission, string module)
        {
            return _authQueries.GetPermissions().FirstOrDefault(p => p.Name == permission && p.Module == module);
        }

        //Temporary solution for Closed Beta R1.0
        public void CreateBrand(Guid brandId, Guid licenseeId, string timeZoneId)
        {
            _securityRepository.Brands.Add(new Core.Security.Data.Brand
            {
                Id = brandId,
                LicenseeId = licenseeId,
                TimeZoneId = timeZoneId,
            });

            _securityRepository.SaveChanges();
        }
    }
}