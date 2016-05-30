using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.GameApi.ServiceContracts;
using AFT.RegoV2.GameApi.Services;
using Microsoft.Practices.Unity;
using AFT.RegoV2.GameApi.Interfaces;

namespace AFT.RegoV2.GameApi.Attributes
{
    public class ValidateTokenDataAttribute : BaseErrorAttribute
    {
        [Dependency]
        internal ITokenProvider TokenProvider { get; set; }
        [Dependency]
        internal ITokenValidationProvider TokenValidation { get; set; }

        public override void OnActionExecuting(HttpActionContext context)
        {
            Exception exception = null;
            try
            {
                foreach (var arg in context.ActionArguments.Values)
                {
                    var req = arg as IAuthTokenHolder;
                    if (req == null) continue;

                    var tokenData = TokenProvider.Decrypt(req.AuthToken);
                    TokenValidation.ValidateToken(tokenData.ToString());

                    return;
                }
            }
            catch (Exception current)
            {
                exception = current;
            }

            var executedContext = 
                new HttpActionExecutedContext(context,
                    exception ?? new ValidationException("Authentication token not found"));
            
            context.Response = GetResponseByException(executedContext);
        }
    }
}