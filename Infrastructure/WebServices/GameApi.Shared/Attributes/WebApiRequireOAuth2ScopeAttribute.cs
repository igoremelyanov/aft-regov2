﻿using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Shared.Extensions;
using AFT.RegoV2.Infrastructure.OAuth2;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.Messages;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;

namespace AFT.RegoV2.GameApi.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class WebApiRequireOAuth2ScopeAttribute : AuthorizationFilterAttribute
    {
        private readonly string[] _oauth2Scopes;

        public WebApiRequireOAuth2ScopeAttribute(params string[] oauth2Scopes)
        {
            _oauth2Scopes = oauth2Scopes;
        }

        [Dependency("authServer")]
        internal ICryptoKeyPair AuthServer { get; set; }
        [Dependency("dataServer")]
        internal ICryptoKeyPair DataServer { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var tokenAnalyzer = 
                    new StandardAccessTokenAnalyzer(
                            AuthServer.PublicSigningKey,
                            DataServer.PrivateEncryptionKey);
                var oauth2ResourceServer = new DotNetOpenAuth.OAuth2.ResourceServer(tokenAnalyzer);
                ((ApiController)actionContext.ControllerContext.Controller).User = 
                    oauth2ResourceServer.GetPrincipal(actionContext.Request.GetRequestBase(), _oauth2Scopes) as PlayerPrincipal;
            }
            catch (ProtocolFaultResponseException ex)
            {
                HandleUnauthorizedRequest(actionContext, ex);
            }
        }

        protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext, ProtocolFaultResponseException ex)
        {
            var response = ex.CreateErrorResponse();
            UnauthorizedResponse error = ex.ErrorResponseMessage as UnauthorizedResponse;
            if (error != null)
            {
                response.Body = JsonConvert.SerializeObject(error.ToGameApiUnauthorizedErrorResponse());
                response.Headers[System.Net.HttpResponseHeader.ContentType] = "application/json";
            }
            HttpContext context = actionContext.Request.GetHttpContext();
            response.Respond(context);

            // these two lines to ensure IIS doesn't mess with our response body
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.Status = context.Response.Status;

            context.Response.End();
        }
    }

    public static class GameApiUnauthorizedErrorResponseExtensions
    {
        public static object ToGameApiUnauthorizedErrorResponse(this UnauthorizedResponse response)
        {
            return (new
            {
                err = GameApiErrorCode.InvalidToken,
                errdesc = response.ErrorDescription,
                error_uri = response.ErrorUri
            });
        }
    }
}