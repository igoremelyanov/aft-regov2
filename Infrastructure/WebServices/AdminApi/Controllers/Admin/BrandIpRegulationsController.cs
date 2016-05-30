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
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Interfaces;
using AutoMapper;
using EditBrandIpRegulationData = AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations.EditBrandIpRegulationData;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    public class BrandIpRegulationsController : BaseApiController
    {
        private readonly BrandIpRegulationService _service;
        private readonly BrandQueries _brands;
        private readonly IAdminQueries _adminQueries;

        public BrandIpRegulationsController(
            BrandIpRegulationService service,
            BrandQueries brands,
            IAuthQueries authQueries,
            IAdminQueries adminQueries)
            : base(authQueries, adminQueries)
        {
            _service = service;
            _brands = brands;
            _adminQueries = adminQueries;

            Mapper.CreateMap<BrandIpRegulation, BrandIpRegulationDTO>()
                .ForMember(dest => dest.Licensee, opt => opt.ResolveUsing(src => _brands.GetLicensees().First(l => l.Id == src.LicenseeId).Name))
                .ForMember(dest => dest.Brand, opt => opt.ResolveUsing(src => _brands.GetBrands().Where(b => b.Licensee.Id == src.LicenseeId).First(b => b.Id == src.BrandId).Name))
                .ForMember(dest => dest.RedirectionUrl, opt => opt.ResolveUsing(src => src.BlockingType == "Redirection" ? src.RedirectionUrl : ""))
                .ForMember(dest => dest.CreatedBy, opt => opt.ResolveUsing(src => (src.CreatedBy != null ? src.CreatedBy.Username : null) ?? String.Empty))
                .ForMember(dest => dest.UpdatedBy, opt => opt.ResolveUsing(src => (src.UpdatedBy != null ? src.UpdatedBy.Username : null) ?? String.Empty));
        }

        [HttpGet]
        [Route(AdminApiRoutes.IsIpAddressUniqueInBrandIpRegulations)]
        public IHttpActionResult IsIpAddressUnique(string ipAddress)
        {
            return Ok(_service.IsIpAddressUnique(ipAddress));
        }

        [HttpGet]
        [Route(AdminApiRoutes.IsIpAddressBatchUniqueInBrandIpRegulations)]
        public IHttpActionResult IsIpAddressBatchUnique(string ipAddressBatch)
        {
            var ipAddresses = ipAddressBatch.Replace("\n", string.Empty).Split(';');

            return Ok(ipAddresses.Select(ip => _service.IsIpAddressUnique(ip))
                .Aggregate(true, (current, result) => current && result));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetLicenseeBrandsInBrandIpRegulations)]
        public IHttpActionResult GetLicenseeBrands([FromUri] Guid licenseeId, bool useBrandFilter)
        {
            var brands = _brands.GetBrands().Where(b => b.Licensee.Id == licenseeId);

            if (useBrandFilter)
            {
                var brandFilterSelections = _adminQueries.GetBrandFilterSelections();

                brands = brands.Where(b => brandFilterSelections.Contains(b.Id));
            }

            return Ok(new
            {
                Brands = brands.OrderBy(l => l.Name).Select(l => new { l.Name, l.Id })
            });
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route(AdminApiRoutes.ListBrandIpRegulations)]
        public IHttpActionResult List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.BrandIpRegulationManager);

            return Ok(SearchData(searchPackage));
        }

        [HttpGet]
        [Route(AdminApiRoutes.GetEditDataInBrandIpRegulations)]
        public IHttpActionResult GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                var licenseeFilterSelections = _adminQueries.GetLicenseeFilterSelections();

                return Ok(new
                {
                    Model = (EditBrandIpRegulationData)null,
                    Licensees = _brands.GetLicensees()
                        .Where(l => l.Brands.Any() && licenseeFilterSelections.Contains(l.Id))
                        .OrderBy(l => l.Name)
                        .Select(l => new { l.Id, l.Name }),
                    BlockingTypes = ConstantsHelper.GetConstantsDictionary<IpRegulationConstants.BlockingTypes>()
                });
            }

            var ipRegulation = _service.GetIpRegulation(id.Value);
            var model = Mapper.Map<Core.Common.Data.Admin.EditBrandIpRegulationData>(ipRegulation);

            return Ok(new
            {
                Model = model,
                Licensees = _brands.GetLicensees()
                    .OrderBy(l => l.Name)
                    .Select(l => new { l.Id, l.Name }),
                BlockingTypes = ConstantsHelper.GetConstantsDictionary<IpRegulationConstants.BlockingTypes>()
            });
        }

        [HttpPost]
        [Route(AdminApiRoutes.CreateIpRegulationInBrandIpRegulations)]
        public IHttpActionResult CreateIpRegulation(Core.Common.Data.Admin.AddBrandIpRegulationData data)
        {
            VerifyPermission(Permissions.Create, Modules.BrandIpRegulationManager);

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

            foreach (var assignedBrand in data.AssignedBrands)
            {
                foreach (var address in addreses)
                {
                    var ipRegulationData = Mapper.DynamicMap<AddBrandIpRegulationData>(data);

                    ipRegulationData.IpAddress = address;
                    ipRegulationData.BrandId = assignedBrand;

                    var result = _service.ValidateAddBrandData(ipRegulationData);

                        if (!result.IsValid)
                            return Ok(new { Result = "failure", Data = result.Errors });

                    _service.CreateIpRegulation(ipRegulationData);
                }
            }

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.UpdateIpRegulationInBrandIpRegulations)]
        public IHttpActionResult UpdateIpRegulation(Core.Common.Data.Admin.EditBrandIpRegulationData data)
        {
            VerifyPermission(Permissions.Update, Modules.BrandIpRegulationManager);

            if (!ModelState.IsValid)
            {
                var messages = ModelState.Where(p => p.Value.Errors.Count > 0)
                        .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                return Ok(new { Result = "failure", Data = messages });
            }

            var ipRegulationData = Mapper.DynamicMap<EditBrandIpRegulationData>(data);
            _service.UpdateIpRegulation(ipRegulationData);

            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route(AdminApiRoutes.DeleteIpRegulationInBrandIpRegulations)]
        public IHttpActionResult DeleteIpRegulation(DeleteBrandIpRegulationData data)
        {
            VerifyPermission(Permissions.Delete, Modules.BrandIpRegulationManager);

            _service.DeleteIpRegulation(data.Id);

            return Ok(new { result = "success" });
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _adminQueries.GetBrandFilterSelections();
            var ipRegulations = _service.GetIpRegulations().Where(x => brandFilterSelections.Contains(x.BrandId));
            var brandIpReulationDto = Queryable.AsQueryable(ipRegulations.Select(Mapper.Map<BrandIpRegulationDTO>));
            var dataBuilder = new SearchPackageDataBuilder<BrandIpRegulationDTO>(searchPackage, brandIpReulationDto);

            return dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.Licensee,
                    r.Brand,
                    r.IpAddress,
                    r.Description,
                    r.BlockingType,
                    r.RedirectionUrl,
                    r.CreatedBy,
                    r.CreatedDate,
                    r.UpdatedBy,
                    r.UpdatedDate
                })
                .GetPageData(r => r.IpAddress);
        }

        private class BrandIpRegulationDTO
        {
            public Guid Id { get; set; }
            public string Licensee { get; set; }
            public string Brand { get; set; }
            public string IpAddress { get; set; }
            public string Restriction { get; set; }
            public string BlockingType { get; set; }
            public string RedirectionUrl { get; set; }
            public string Description { get; set; }
            public string CreatedBy { get; set; }
            public DateTimeOffset? CreatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTimeOffset? UpdatedDate { get; set; }
        }
    }
}