using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.MemberApi.Filters;
using AFT.RegoV2.MemberApi.Interface;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberApi.Provider
{
    public class AuthServerProvider : OAuthAuthorizationServerProvider
    {
        private readonly IUnityContainer _container;
        public AuthServerProvider(IUnityContainer container)
        {
            _container = container;
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // TODO: Implement this when each member website has their own ClientID and Client Secret
            // For now - validate everyone
            
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-CA");

            // read additional data from the request body
            var requestData = await context.Request.ReadFormAsync();

            var brandId = Guid.Parse(requestData["BrandId"]);
            var ipAddress = requestData["IpAddress"];
            var jsonHeaders = requestData["BrowserHeaders"];

            var headers = string.IsNullOrWhiteSpace(jsonHeaders)
                ? new Dictionary<string, string>()
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonHeaders);

            MemberApiException exception;
            try
            {
                var loginContext = new LoginRequestContext
                {
                    BrandId = brandId,
                    IpAddress = ipAddress,
                    BrowserHeaders = headers
                };
                var validationResult = _container.Resolve<PlayerCommands>().Login(context.UserName, context.Password, loginContext);

                if (validationResult.IsValid)
                {
                    var player = _container.Resolve<IPlayerRepository>().Players.Single(p => p.Username == context.UserName);
                    var identity = _container.Resolve<ClaimsIdentityProvider>().GetActorIdentity(player.Id, context.Options.AuthenticationType);

                    context.Validated(identity);
                    context.Request.Context.Authentication.SignIn(identity);
                    return;
                }

                exception = new MemberApiException
                {
                    ErrorCode = validationResult.Errors[0].ErrorMessage,
                    ErrorMessage = validationResult.Errors[0].ErrorMessage,
                    Violations = validationResult.Errors.Select(
                        x => new ValidationErrorField
                        {
                            ErrorCode = x.ErrorCode,
                            ErrorMessage = x.ErrorMessage,
                            FieldName = x.PropertyName,
                            Params = x.FormattedMessageArguments
                        }
                        ).ToList()
                };
            }
            catch (Exception ex)
            {
                exception = new MemberApiException
                {
                    ErrorCode = ex.Message,
                    ErrorMessage = ex.Message,
                    Violations = new []
                    {
                        new ValidationErrorField
                        {
                            ErrorCode = ex.Message,
                            ErrorMessage = ex.Message,
                            FieldName = string.Empty
                        }
                    }
                };
            }
            context.Rejected();

            var sError = JsonConvert.SerializeObject(exception);
            context.SetError("Login error", sError);
            context.Response.Headers.Add(InvalidLoginOwinMiddleware.InvalidLoginHeader, new[] {sError});
        }
    }
}