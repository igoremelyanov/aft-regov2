using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.MemberApi.Interface.Security;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.Shared;
using AutoMapper;
using AFT.RegoV2.Core.Security.Interface.Data;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class SecurityController : BaseApiController
    {
        private readonly BrandIpRegulationService _regulations;
        private readonly IBrandRepository _brands;
        private readonly LoggingService _logging;

        static SecurityController()
        {
            Mapper.CreateMap<ApplicationErrorRequest, Error>();
            Mapper.CreateMap<Core.Security.Data.IpRegulations.VerifyIpResult, VerifyIpResult>();
        }

        public SecurityController(
            BrandIpRegulationService regulations, 
            IBrandRepository brands,
            LoggingService logging)
        {
            _regulations = regulations;
            _brands = brands;
            _logging = logging;
        }

        [AllowAnonymous]
        [HttpPost]
        public VerifyIpResponse VerifyIp(VerifyIpRequest request)
        {
            var brand = _brands.Brands.SingleOrDefault(b => b.Code == request.BrandName);

            if (brand == null)
                throw new RegoValidationException(ErrorMessagesEnum.InvalidBrandCode.ToString());

            Core.Security.Data.IpRegulations.VerifyIpResult result;
            try
            {
                result = _regulations.VerifyIpAddress(request.IpAddress, brand.Id);
            }
            catch (FormatException)
            {
                throw new RegoValidationException(ErrorMessagesEnum.InvalidIpAddress.ToString());
            }

            var verifyIpResult = Mapper.Map<VerifyIpResult>(result);

            return new VerifyIpResponse(verifyIpResult);
        }



        [AllowAnonymous]
        [HttpPost]
        public ApplicationErrorResponse LogApplicationError(ApplicationErrorRequest request)
        {
            var error = Mapper.Map<Error>(request);
            _logging.Log(error);
            return new ApplicationErrorResponse();
        }
    }
}