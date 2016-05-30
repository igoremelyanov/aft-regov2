using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using Microsoft.Practices.Unity;
using AFT.RegoV2.GameApi.Shared.Services;

namespace AFT.RegoV2.GameApi.Shared.Attributes
{
    public abstract class BaseErrorAttribute : ActionFilterAttribute
    {
        [Dependency]
        public IGameProviderLog Log { get; set; }
        [Dependency]
        public IErrorManager ErrorManager { get; set; }
        [Dependency]
        public IJsonSerializationProvider Json { get; set; }
        [Dependency]
        public IUnityContainer Container { get; set; }

        protected Type GetReturnType(HttpActionExecutedContext context)
        {
            Type returnType = null;
            if (context.ActionContext != null &&
                context.ActionContext.ActionDescriptor != null &&
                context.ActionContext.ActionDescriptor.ReturnType != null)
            {
                returnType = context.ActionContext.ActionDescriptor.ReturnType;
            }
            return returnType;
        }
        protected HttpResponseMessage GetResponseByException(HttpActionExecutedContext context)
        {
            string description;
            var code = ErrorManager.GetErrorCodeByException(context.Exception, out description);

            if(code == GameApiErrorCode.SystemError) Log.LogError(description, context.Exception);
            else Log.LogWarn(description);

            var returnType = GetReturnType(context);
            
            if (returnType != null)
            {
                if (typeof (IGameApiErrorDetails).IsAssignableFrom(returnType) && 
                    returnType.GetConstructor(Type.EmptyTypes) != null)
                {
                    var error = (IGameApiErrorDetails)Activator.CreateInstance(returnType);
                    error.ErrorCode = code;
                    error.ErrorDescription = description;

                    return context.Request.CreateResponse(HttpStatusCode.InternalServerError, error);
                }
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = 
                    new StringContent(Json.SerializeToString(new BetCommandResponse
                        {
                            ErrorCode = code,
                            ErrorDescription = description
                        }))
            };
        }

    }
}