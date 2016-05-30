using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class AdminActivityLogController : BaseApiController
    {
        private readonly ReportQueries _reportQueries;

        public AdminActivityLogController(
            ReportQueries reportQueries,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _reportQueries = reportQueries;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListAdminActivityLog)]
        public IHttpActionResult Data([FromUri] SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<AdminActivityLog>(searchPackage, _reportQueries.GetAdminActivityLog());
            var data = dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.Category.ToString(),
                    r.ActivityDone,
                    r.PerformedBy,
                    r.DatePerformed.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                    r.Remarks
                })
                .GetPageData(r => r.DatePerformed);
            return Ok(data);
        }
    }
}