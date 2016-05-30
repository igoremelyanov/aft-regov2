using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using AFT.RegoV2.AdminApi.Filters;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Providers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using ValidationErrorField = AFT.RegoV2.AdminApi.Interface.Common.ValidationErrorField;

namespace AFT.RegoV2.AdminApi.Provider
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

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            AdminApiException exception;

            try
            {
                var admin = _container.Resolve<IAdminQueries>().GetAdminByName(context.UserName);
                if (admin == null)
                    throw new Exception("Admin not found");

                var authQueries = _container.Resolve<IAuthQueries>();
                if (!authQueries.GetValidationResult(new LoginActor { ActorId = admin.Id, Password = context.Password }).IsValid)
                    throw new Exception("Incorrect username or password");

                var identity = _container.Resolve<ClaimsIdentityProvider>().GetActorIdentity(admin.Id, context.Options.AuthenticationType);
                context.Validated(identity);
                context.Request.Context.Authentication.SignIn(identity);
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                exception = new AdminApiException
                {
                    ErrorCode = ex.Message,
                    ErrorMessage = ex.Message,
                    Violations = new[]
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
            context.Response.Headers.Add(InvalidLoginOwinMiddleware.InvalidLoginHeader, new[] { sError });
            return Task.FromResult(0);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            // TODO: Implement this when each member website has their own ClientID
            //var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            //var currentClient = context.ClientId;

            //// enforce client binding of refresh token
            //if (originalClient != currentClient)
            //{
            //    context.Rejected();
            //    return;
            //}

            var identity = new ClaimsIdentity(context.Ticket.Identity);
            var newTicket = new AuthenticationTicket(identity, context.Ticket.Properties);
            context.Validated(newTicket);
            return Task.FromResult(0);
        }
    }

    public class RefreshTokenProvider : IAuthenticationTokenProvider
    {
        // TODO: Later store refresh tokens to DB
        private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens =
            new ConcurrentDictionary<string, AuthenticationTicket>();

        public Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var guid = Guid.NewGuid().ToString();

            // maybe only create a handle the first time, then re-use for same client
            // copy properties and set the desired lifetime of refresh token
            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = DateTime.UtcNow.AddYears(1)
            };
            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            _refreshTokens.TryAdd(guid, refreshTokenTicket);

            // consider storing only the hash of the handle
            context.SetToken(guid);

            return Task.FromResult(0);
        }

        public Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            AuthenticationTicket ticket;
            if (_refreshTokens.TryRemove(context.Token, out ticket))
            {
                context.SetTicket(ticket);
            }
            else
            {
                throw new AdminApiException
                {
                    ErrorCode = "500",
                    ErrorMessage = "Refresh token not found",
                    Violations = new ValidationErrorField[0]
                };
            }

            return Task.FromResult(0);
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}