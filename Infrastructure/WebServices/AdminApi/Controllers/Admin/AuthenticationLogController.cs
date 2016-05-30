using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class AuthenticationLogController : BaseApiController
    {
        private readonly ReportQueries _reportQueries;
        private readonly IAdminQueries _adminQueries;

        public AuthenticationLogController(
            ReportQueries reportQueries, 
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _reportQueries = reportQueries;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListAdminAuthenticationLogs)]
        public IHttpActionResult AdminAuthData([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.AdminAuthenticationLog);

            var dataBuilder = new SearchPackageDataBuilder<AdminAuthenticationLog>(searchPackage, _reportQueries.GetAdminAuthenticationLog());
            var data = dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.DatePerformed,
                    r.PerformedBy,
                    r.IPAddress,
                    r.Success,
                    r.FailReason,
                    r.Headers
                })
                .GetPageData(r => r.DatePerformed);
            return Ok(data);
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListMemberAuthenticationLogs)]
        public IHttpActionResult MemberAuthData([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.MemberAuthenticationLog);

            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var logs = _reportQueries.GetMemberAuthenticationLog().Where(x => brandFilterSelections.Contains(x.BrandId));
            var dataBuilder = new SearchPackageDataBuilder<MemberAuthenticationLog>(searchPackage, logs);

            var data = dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.Brand,
                    r.DatePerformed,
                    r.PerformedBy,
                    r.IPAddress,
                    r.Success,
                    r.FailReason,
                    r.Headers
                })
                .GetPageData(r => r.DatePerformed);

            return Ok(data);

        }
    }
}