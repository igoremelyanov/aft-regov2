using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    [Authorize]
    public class CultureController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly LicenseeQueries _licenseeQueries;
        private readonly ICultureCommands _cultureCommands;

        public CultureController(
            BrandQueries queries,
            BrandCommands commands,
            LicenseeQueries licenseeQueries,
            ICultureCommands cultureCommands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _brandQueries = queries;
            _brandCommands = commands;
            _licenseeQueries = licenseeQueries;
            _cultureCommands = cultureCommands;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListCulture)]
        public IHttpActionResult List([FromUri] SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<Culture>(searchPackage, _brandQueries.GetCultures().ToArray().AsQueryable());

            dataBuilder.Map(
                c => c.Code,
                c => new object[]
                {
                    c.Name,
                    c.Code,
                    c.NativeName,
                    c.Status.ToString(),
                    c.CreatedBy,
                    Format.FormatDate(c.DateCreated, false),
                    c.UpdatedBy,
                    Format.FormatDate(c.DateUpdated, false),
                    c.ActivatedBy,
                    Format.FormatDate(c.DateActivated, false),
                    c.DeactivatedBy,
                    Format.FormatDate(c.DateDeactivated,false),
                    _licenseeQueries.GetLicensees().Any(x => x.Cultures.Any(y => y.Code == c.Code))
                });

            return Ok(dataBuilder.GetPageData(c => c.Code));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetCultureByCode)]
        public IHttpActionResult GetByCode(string code)
        {
            var culture = _brandQueries.GetCulture(code);

            return Ok(new
            {
                culture.Code,
                culture.Name,
                culture.NativeName
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.ActivateCulture)]
        public IHttpActionResult Activate(ActivateCultureData data)
        {
            VerifyPermission(Permissions.Activate, Modules.LanguageManager);
            _brandCommands.ActivateCulture(data.Code, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeactivateCulture)]
        public IHttpActionResult Deactivate(DeactivateCultureData data)
        {
            VerifyPermission(Permissions.Deactivate, Modules.LanguageManager);
            _brandCommands.DeactivateCulture(data.Code, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.SaveCulture)]
        public IHttpActionResult Save(EditCultureData data)
        {
            var newCulture = string.IsNullOrEmpty(data.OldCode);
            VerifyPermission(newCulture ? Permissions.Create : Permissions.Update, Modules.LanguageManager);

            if (!ModelState.IsValid)
                return Ok(ErrorResponse());

            try
            {
                var result = newCulture
                        ? _cultureCommands.Save(data)
                        : _cultureCommands.Edit(data);

                return Ok(new {Result = "success", Data = result});
            }
            catch (Exception e)
            {
                return Ok(new {Result = "failed", Data = e.Message});
            }
        }
    }
}