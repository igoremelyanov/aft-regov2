using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class AdminManagerController : BaseApiController
    {
        private readonly IAdminQueries _adminQueries;
        private readonly IAdminCommands _adminCommands;
        private readonly RoleService _roleService;
        private readonly BrandQueries _brands;

        public AdminManagerController(
            IAdminCommands adminCommands,
            RoleService roleService,
            BrandQueries brands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _adminQueries = adminQueries;
            _adminCommands = adminCommands;
            _roleService = roleService;
            _brands = brands;
        }

        #region Actions

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListUsers)]
        public IHttpActionResult Data([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.AdminManager);

            return Ok(SearchData(searchPackage));
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateUserInAdminManager)]
        public IHttpActionResult CreateUser(AddAdminData data)
        {
            VerifyPermission(Permissions.Create, Modules.AdminManager);

            var validationResult = _adminQueries.GetValidationResult(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _adminCommands.CreateAdmin(data);

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateUserInAdminManager)]
        public IHttpActionResult UpdateUser(EditAdminData data)
        {
            VerifyPermission(Permissions.Update, Modules.AdminManager);

            var validationResult = _adminQueries.GetValidationResult(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _adminCommands.UpdateAdmin(data);

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ResetPasswordInAdminManager)]
        public IHttpActionResult ResetPassword(AddAdminData admin)
        {
            VerifyPermission(Permissions.Update, Modules.AdminManager);

            if (string.Compare(admin.Password, admin.PasswordConfirmation, StringComparison.OrdinalIgnoreCase) != 0)
                return Ok(new { Result = "failure", Data = new RegoException("Passwords does not match") });

            _adminCommands.ChangePassword(admin.Id, admin.Password);

            return Ok(new { Result = "success", Data = admin });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivateUserInAdminManager)]
        public IHttpActionResult Activate(ActivateUserData data)
        {
            VerifyPermission(Permissions.Activate, Modules.AdminManager);

            _adminCommands.Activate(data);

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivateUserInAdminManager)]
        public IHttpActionResult Deactivate(DeactivateUserData data)
        {
            VerifyPermission(Permissions.Deactivate, Modules.AdminManager);

            _adminCommands.Deactivate(data);

            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetUserEditDataInAdminManager)]
        public IHttpActionResult GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();
                var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

                return Ok(new
                {
                    Licensees = _brands.GetLicensees()
                        .Where(l => licenseeFilterSelections.Contains(l.Id))
                        .Select(l => new { l.Id, l.Name }),
                    Brands = _brands.GetBrands()
                        .Where(b => brandFilterSelections.Contains(b.Id))
                        .Select(b => new { b.Id, b.Code, b.Name }),
                    Currencies = _brands.GetCurrencies().Select(c => new { c.Code, c.Name })
                });
            }

            var user = _adminQueries.GetAdminById(id.Value);

            var userData = Mapper.Map<AddAdminData>(user);

            userData.AssignedLicensees = user.Licensees.Select(l => l.Id).ToList();
            userData.AllowedBrands = user.AllowedBrands.Select(b => b.Id).ToList();
            userData.Currencies = user.Currencies.Select(c => c.Currency).ToList();

            return Ok(new
            {
                User = userData,
                Licensees = _brands.GetLicensees().Select(l => new { l.Id, l.Name }),
                Currencies = _brands.GetCurrencies().Select(c => new { c.Code, c.Name }),
                Brands = _brands.GetBrands().Select(b => new { b.Id, b.Code, b.Name })
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.GetLicenseeDataInAdminManager)]
        public IHttpActionResult GetLicenseeData(GetLicenseeData request)
        {
            VerifyPermission(Permissions.View, Modules.RoleManager);
            VerifyPermission(Permissions.View, Modules.AdminManager);

            var brands = _brands.GetBrands().Where(b => request.Licensees.Contains(b.Licensee.Id));
            var roles = _roleService.GetRoles().Where(r => r.Licensees.Select(l => l.Id).Intersect(request.Licensees).Any());
            var currencies = _brands.GetLicensees().Where(l => request.Licensees.Contains(l.Id)).SelectMany(l => l.Currencies).Distinct();

            if (request.UseBrandFilter)
            {
                var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

                brands = brands.Where(b => brandFilterSelections.Contains(b.Id));
            }

            return Ok(new
            {
                Brands = brands.OrderBy(l => l.Name).Select(l => new { l.Id, l.Name }),
                Roles = roles.Select(r => new { r.Id, r.Name }),
                Currencies = currencies.OrderBy(c => c.Code).Select(c => new { c.Code, c.Name })
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SaveBrandFilterSelectionInAdminManager)]
        public IHttpActionResult SaveBrandFilterSelection(BrandFilterSelectionData data)
        {
            _adminCommands.SetBrandFilterSelections(data.Brands);

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SaveLicenseeFilterSelectionInAdminManager)]
        public IHttpActionResult SaveLicenseeFilterSelection(LicenseeFilterSelectionData data)
        {
            _adminCommands.SetLicenseeFilterSelections(data.Licensees);

            return Ok(new { result = "success" });
        }

        #endregion

        #region Helper methods

        private object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var users = _adminQueries.GetAdmins().Where(x => x.AllowedBrands.Any(y => brandFilterSelections.Contains(y.Id)));
            var dataBuilder = new SearchPackageDataBuilder<Core.Security.Data.Users.Admin>(searchPackage, users);

            return dataBuilder
                .Map(user => user.Id, user => GetUserCell(user))
                .GetPageData(user => user.Username);
        }

        private object GetUserCell(Core.Security.Data.Users.Admin admin)
        {
            return new[]
            {
                admin.Username,
                admin.FirstName,
                admin.LastName,
                admin.Language,
                admin.Role.Name,
                admin.IsActive ? "Active" : "Inactive",
                admin.Description
            };
        }

        #endregion
    }
}