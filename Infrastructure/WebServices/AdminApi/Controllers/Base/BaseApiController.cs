using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Interface.Auth;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Base;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Attributes;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.AdminApi.Controllers.Base
{
    [ForceJsonFormatter]
    [Authorize]
    public class BaseApiController : ApiController
    {
        private readonly IAuthQueries _authQueries;
        private readonly IAdminQueries _adminQueries;

        public BaseApiController()
        {
        }

        public BaseApiController(IAuthQueries authQueries, IAdminQueries adminQueries)
        {
            Thread.CurrentPrincipal = (ClaimsPrincipal) User;
            _authQueries = authQueries;
            _adminQueries = adminQueries;
        }

        protected string Username
        {
            get
            {
                var principal = (ClaimsPrincipal)User;
                var username = principal.Claims.Any() ? (from c in principal.Claims where c.Type == ClaimTypes.Name select c.Value).Single() : "";
                return username;
            }
        }

        protected Guid UserId
        {
            get
            {
                var principal = (ClaimsPrincipal)User;
                var userId = principal.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier) ? (from c in principal.Claims where c.Type == ClaimTypes.NameIdentifier select c.Value).Single() : Guid.Empty.ToString();
                return new Guid(userId);
            }
        }

        protected void VerifyPermission(string permissionName, string module)
        {
            if (!_authQueries.VerifyPermission(UserId, permissionName, module))
                throw new HttpException(403, "Access forbidden");
        }

        protected void VerifyAnyPermission(params Permission[] permissions)
        {
            if (permissions.All(permission => !_authQueries.VerifyPermission(UserId, permission.Name, permission.Module)))
                throw new HttpException(403, "Access forbidden");
        }

        protected void CheckBrand(Guid brandId)
        {
            if (!_adminQueries.IsBrandAllowed(UserId, brandId))
                throw new HttpException(403, "Access forbidden");
        }

        protected static string ToCamelCase(string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public object ValidationExceptionResponse(IList<ValidationFailure> e)
        {
            var fields = e
                .GroupBy(x => x.PropertyName)
                .Select(x => new
                {
                    Name = ToCamelCase(x.Key),
                    Errors = x.Select(y => y.ErrorMessage).ToArray()
                }).ToArray();

            return new { Result = "failed", Fields = fields };
        }

        public T ValidationErrorResponse<T>(ValidationResult validationResult)
        {
            var obj = new ValidationResponseBase
            {
                Success = false,
                Errors = validationResult.Errors.Select(er =>
                new ValidationError { PropertyName = Char.ToLowerInvariant(er.PropertyName[0]) + er.PropertyName.Substring(1), ErrorMessage = er.ErrorMessage })
            };

            return Mapper.DynamicMap<T>(obj);
        }

        public object ErrorResponse()
        {
            var fields = ModelState.Where(p => p.Value.Errors.Count > 0)
                .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
            return new { Result = "failed", Fields = fields };
        }
    }
}
