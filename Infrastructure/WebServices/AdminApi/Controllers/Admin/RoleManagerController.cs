using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class RoleManagerController : BaseApiController
    {
        private readonly RoleService _service;
        private readonly BrandQueries _brands;
        private readonly IAdminQueries _adminQueries;
        private readonly IAuthQueries _authQueries;

        public RoleManagerController(
            RoleService service,
            BrandQueries brands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _service = service;
            _brands = brands;
            _adminQueries = adminQueries;
            _authQueries = authQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListRoles)]
        public IHttpActionResult Data([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.RoleManager);
            VerifyPermission(Permissions.View, Modules.AdminManager);

            return Ok(SearchData(searchPackage));
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateRoleInRoleManager)]
        public IHttpActionResult CreateRole(AddRoleData data)
        {
            VerifyPermission(Permissions.Create, Modules.RoleManager);

            _service.CreateRole(data);

            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetRoleInRoleManager)]
        public IHttpActionResult GetRole(Guid id)
        {
            var role = _service.GetRoleById(id);
            var roleData = Mapper.DynamicMap<AddRoleData>(role);

            roleData.CheckedPermissions = _authQueries.GetRolePermissions(role.Id);
            roleData.AssignedLicensees = role.Licensees.Select(l => l.Id).ToList();

            return Ok(roleData);
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetEditDataInRoleManager)]
        public IHttpActionResult GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();

                return Ok(new
                {
                    Licensees = _brands.GetLicensees()
                        .Where(l => licenseeFilterSelections.Contains(l.Id))
                        .OrderBy(l => l.Name)
                        .Select(l => new { l.Id, l.Name })
                });
            }

            var role = _service.GetRoleById(id.Value);
            var roleData = Mapper.DynamicMap<EditRoleData>(role);

            roleData.CheckedPermissions = _authQueries.GetRolePermissions(role.Id);
            roleData.AssignedLicensees = role.Licensees.Select(l => l.Id).ToList();

            return Ok(new
            {
                Role = roleData,
                Licensees = _brands.GetLicensees()
                    .OrderBy(l => l.Name)
                    .Select(l => new { l.Id, l.Name })
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateRoleInRoleManager)]
        public IHttpActionResult UpdateRole(EditRoleData data)
        {
            VerifyPermission(Permissions.Update, Modules.RoleManager);

            _service.UpdateRole(data);

            return Ok(new
            {
                result = "success",
                data = new
                {
                    Role = data,
                    Licensees = _brands.GetLicensees().Select(l => new { l.Id, l.Name })
                }
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.GetLicenseeDataInRoleManager)]
        public IHttpActionResult GetLicenseeData(IEnumerable<Guid> licensees)
        {
            VerifyPermission(Permissions.View, Modules.RoleManager);
            VerifyPermission(Permissions.View, Modules.AdminManager);

            var filter = licensees ?? new List<Guid>();
            var roles = _service.GetRoles().Where(r => r.Licensees.Select(l => l.Id).Intersect(filter).Any());

            return Ok(new
            {
                Roles = roles.Select(r => new { r.Id, r.Name })
            });
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();
            var roles = _service.GetRoles().Where(r => r.Licensees.Any(l => licenseeFilterSelections.Contains(l.Id)));
            var dataBuilder = new SearchPackageDataBuilder<Role>(searchPackage, roles);

            return dataBuilder
                .Map(role => role.Id, role => GetRoleCell(role)) 
                .GetPageData(role => role.Name);
        }

        private object GetRoleCell(Role role)
        {
            return new[]
            {
                role.Code,
                role.Name,
                role.Description,
                role.CreatedBy != null ? role.CreatedBy.Username : string.Empty,
                Format.FormatDate(role.CreatedDate),
                role.UpdatedBy != null ? role.UpdatedBy.Username : string.Empty,
                Format.FormatDate(role.UpdatedDate)
            };
        }
    }
}