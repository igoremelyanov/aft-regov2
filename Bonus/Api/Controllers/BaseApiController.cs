using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    [Authorize]
    public class BaseApiController : ApiController
    {
        public BaseApiController()
        {
            Thread.CurrentPrincipal = (ClaimsPrincipal)User;
        }

        public T ValidationErrorResponse<T>(ValidationResult validationResult)
        {
            var obj = new ValidationResponseBase
            {
                Success = false,
                Errors = validationResult.Errors.Select(er => new ValidationError
                {
                    PropertyName = er.PropertyName,
                    ErrorMessage = er.ErrorMessage
                }).ToList()
            };

            return Mapper.DynamicMap<T>(obj);
        }
    }
}
