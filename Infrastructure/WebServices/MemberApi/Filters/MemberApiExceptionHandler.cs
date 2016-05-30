using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using AFT.RegoV2.MemberApi.Interface.Exceptions;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Filters
{
    public class MemberApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            base.Handle(context);

            if (context.IsExceptionOfType<RegoException>())
            {
                context.Result = context.CreateErrorResponseFromException<RegoException>(
                    HttpStatusCode.InternalServerError);
            }
            else if (context.IsExceptionOfType<RegoValidationException>())
            {
                context.Result = context.CreateValidationResponseFromException(
                    HttpStatusCode.BadRequest);
            }
        }
    }

    public static class ExceptionHandlerContextEx
    {
        public static bool IsExceptionOfType<T>(this ExceptionHandlerContext actionExecutedContext)
            where T : Exception
        {
            return actionExecutedContext.Exception != null
                   && actionExecutedContext.Exception.GetType() == typeof(T);
        }


        public static IHttpActionResult CreateErrorResponseFromException<T>(this ExceptionHandlerContext actionExecutedContext,
            HttpStatusCode code)
            where T : Exception
        {
            var typedException = actionExecutedContext.Exception as T;

            if (typedException == null)
            {
                throw new RegoException("Unknown exception type");
            }

            var errorMessagError = new HttpError(typedException.Message);
            return new ResponseMessageResult(
                actionExecutedContext.Request.CreateErrorResponse(code, errorMessagError));
        }

        public static IHttpActionResult CreateValidationResponseFromException(this ExceptionHandlerContext actionExecutedContext,
            HttpStatusCode code)
        {
            var typedException = actionExecutedContext.Exception as RegoValidationException;

            if (typedException == null)
            {
                throw new RegoException("Unknown validation exception type");
            }

            var errorMessagError = new HttpError(typedException.Message);
            errorMessagError.AddValidationErrors(typedException.ValidationErrors);
            return new ResponseMessageResult(
                actionExecutedContext.Request.CreateErrorResponse(code, errorMessagError));
        }
    }
}