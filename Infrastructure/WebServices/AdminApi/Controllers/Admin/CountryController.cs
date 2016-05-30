using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    [Authorize]
    public class CountryController : BaseApiController
    {
        private readonly BrandQueries _queries;
        private readonly BrandCommands _commands;

        public CountryController(
            BrandQueries queries,
            BrandCommands commands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _queries = queries;
            _commands = commands;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListCountries)]
        public IHttpActionResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.CountryManager);

            var dataBuilder = new SearchPackageDataBuilder<Country>(searchPackage, _queries.GetAllCountries().AsQueryable());

            dataBuilder
                .Map(c => c.Code,
                    c =>
                        new[]
                        {
                            c.Name,
                            c.Code
                        }
                );

            return Ok(dataBuilder.GetPageData(c => c.Name));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetCountryByCode)]
        public IHttpActionResult GetByCode(string code)
        {
            return Ok(_queries.GetCountry(code));
        }

        [HttpPost]
        [Route(AdminApiRoutes.SaveCountry)]
        public IHttpActionResult Save(EditCountryData data)
        {
            var oldCode = data.OldCode;
            var newCountry = string.IsNullOrEmpty(oldCode);
            VerifyPermission(newCountry ? Permissions.Create : Permissions.Update, Modules.CountryManager);

            if (!newCountry)
            {
                if (_queries.GetCountry(oldCode) == null)
                {
                    ModelState.AddModelError("Id", "{\"text\": \"app:common.invalidId\"}");
                }
            }

            if (_queries.GetCountries().Any(c => c.Name == data.Name && c.Code != oldCode))
            {
                ModelState.AddModelError("Name", "{\"text\": \"app:common.nameUnique\"}");
            }

            if (_queries.GetCountries().Any(c => c.Code == data.Code && c.Code != oldCode))
            {
                ModelState.AddModelError("Code", "{\"text\": \"app:common.codeUnique\"}");
            }

            if (!ModelState.IsValid)
            {
                var fields = ModelState.Where(p => p.Value.Errors.Count > 0)
                        .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                return Ok(new { Result = "failed", Fields = fields });
            }

            if (newCountry)
            {
                _commands.CreateCountry(data.Code, data.Name);
            }
            else
            {
                _commands.UpdateCountry(oldCode, data.Name);
            }

            var messageName = newCountry ? "app:country.created" : "app:country.updated";

            return Ok(new { Result = "success", Data = messageName });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeleteCountry)]
        public IHttpActionResult Delete(DeleteCountryData data)
        {
            VerifyPermission(Permissions.Delete, Modules.CountryManager);

            var country = _queries.GetCountry(data.Code);
            var message = string.Empty;

            if (country == null)
                message = "app:country.notFound";
            else if (_queries.IsCountryAssignedToAnyBrand(data.Code))
                message = "app:country.notDeletedHasBrand";

            if (message != string.Empty)
                return Ok(new { Result = "failure", Data = message });

            _commands.DeleteCountry(data.Code);

            return Ok(new { Result = "success", Data = "app:country.deleted" });
        }
    }
}