using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class AdminIpRegulationsController : BaseApiController
    {
        private readonly BackendIpRegulationService _service;
        private readonly BrandQueries _brands;

        public AdminIpRegulationsController(
            BackendIpRegulationService service,
            BrandQueries brands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _service = service;
            _brands = brands;

            Mapper.CreateMap<AdminIpRegulation, AdminIpRegulationDTO>()
                .ForMember(dest => dest.CreatedBy, opt => opt.ResolveUsing(src => (src.CreatedBy != null ? src.CreatedBy.Username : null) ?? String.Empty))
                .ForMember(dest => dest.UpdatedBy, opt => opt.ResolveUsing(src => (src.UpdatedBy != null ? src.UpdatedBy.Username : null) ?? String.Empty));
        }

        [HttpGet]
        [Route(AdminApiRoutes.IsIpAddressUniqueInAdminIpRegulations)]
        public IHttpActionResult IsIpAddressUnique(string ipAddress)
        {
            return Ok(_service.IsIpAddressUnique(ipAddress));
        }

        [HttpGet]
        [Route(AdminApiRoutes.IsIpAddressBatchUniqueInAdminIpRegulations)]
        public IHttpActionResult IsIpAddressBatchUnique(string ipAddressBatch)
        {
            var ipAddresses = ipAddressBatch.Replace("\n", string.Empty).Split(';');

            return Ok(ipAddresses.Select(ip => _service.IsIpAddressUnique(ip))
                .Aggregate(true, (current, result) => current && result));
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListAdminIpRegulations)]
        public IHttpActionResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.BackendIpRegulationManager);

            return Ok(SearchData(searchPackage));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetEditDataInAdminIpRegulations)]
        public IHttpActionResult GetEditData(Guid? id = null)
        {
            EditAdminIpRegulationData data = null;
            if (id.HasValue)
            {
                var ipRegulation = _service.GetIpRegulation(id.Value);
                data = Mapper.Map<EditAdminIpRegulationData>(ipRegulation);
            }

            return Ok(new
            {
                Model = data,
                Licensees = _brands.GetLicensees().Select(l => new { l.Id, l.Name }),
                BlockingTypes = ConstantsHelper.GetConstantsDictionary<IpRegulationConstants.BlockingTypes>()
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateIpRegulationInAdminIpRegulations)]
        public IHttpActionResult CreateIpRegulation(EditAdminIpRegulationData data)
        {
            VerifyPermission(Permissions.Create, Modules.BackendIpRegulationManager);

            // TODO: Move validation logic to separate class
            var addreses = new List<string>();

            if (string.IsNullOrEmpty(data.IpAddressBatch))
                addreses.Add(data.IpAddress);
            else
            {
                addreses.AddRange(data.IpAddressBatch.Split(';').Select(ip => ip.Trim(new[] { ' ', '\n' })));
            }

            if (!addreses.TrueForAll(ip => _service.IsIpAddressUnique(ip)))
            {
                ModelState.AddModelError("IpAddress", "{\"text\": \"app:admin.messages.duplicateIp\"}");
            }

            if (!ModelState.IsValid)
            {
                var messages = ModelState.Where(p => p.Value.Errors.Count > 0)
                        .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                return Ok(new { Result = "failure", Data = messages });
            }

            foreach (var address in addreses)
            {
                var regulationData = Mapper.DynamicMap<AddBackendIpRegulationData>(data);

                regulationData.IpAddress = address;
                _service.CreateIpRegulation(regulationData);
            }

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateIpRegulationInAdminIpRegulations)]
        public IHttpActionResult UpdateIpRegulation(EditAdminIpRegulationData data)
        {
            VerifyPermission(Permissions.Update, Modules.BackendIpRegulationManager);

            if (!ModelState.IsValid)
            {
                var messages = ModelState.Where(p => p.Value.Errors.Count > 0)
                        .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                return Ok(new { Result = "failure", Data = messages });
            }

            var regulationData = Mapper.DynamicMap<EditBackendIpRegulationData>(data);
            _service.UpdateIpRegulation(regulationData);

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeleteIpRegulationInAdminIpRegulations)]
        public IHttpActionResult DeleteIpRegulation(DeleteAdminIpRegulationData data)
        {
            VerifyPermission(Permissions.Delete, Modules.BackendIpRegulationManager);

            _service.DeleteIpRegulation(data.Id);

            return Ok(new { result = "success" });
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<AdminIpRegulationDTO>(searchPackage,
                _service.GetIpRegulations().ToList().Select(Mapper.Map<AdminIpRegulationDTO>).AsQueryable());

            return dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.IpAddress,
                    r.Description,
                    r.CreatedBy,
                    r.CreatedDate,
                    r.UpdatedBy,
                    r.UpdatedDate
                })
                .GetPageData(r => r.IpAddress);
        }

        private class AdminIpRegulationDTO
        {
            public Guid Id { get; set; }
            public string IpAddress { get; set; }
            public string Description { get; set; }
            public string CreatedBy { get; set; }
            public DateTimeOffset? CreatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTimeOffset? UpdatedDate { get; set; }
        }
    }
}