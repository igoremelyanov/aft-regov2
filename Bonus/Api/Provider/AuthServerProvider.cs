using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared.OAuth2;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Bonus.Api.Provider
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
            string clientId;
            string clientSecret;
            if (context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                var configuration = _container.Resolve<ICommonSettingsProvider>().GetBonusApiCredentials();
                if (clientId == configuration.ClientId && clientSecret == configuration.ClientSecret)
                {
                    context.Validated(clientId);
                }
                else
                {
                    context.Rejected();
                    context.SetError(OAuth2Constants.Errors.InvalidClient, "Client credentials are invalid.");
                }
            }
            else
            {
                context.Rejected();
                context.SetError(OAuth2Constants.Errors.InvalidClient, "Client credentials could not be retrieved.");
            }

            return Task.FromResult<object>(null);
        }

        public override Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            try
            {
                var actorId = context.Request.Headers["ActorId"];
                var identity = _container.Resolve<ClaimsIdentityProvider>().GetActorIdentity(Guid.Parse(actorId), context.Options.AuthenticationType);
                context.Validated(identity);
                context.Request.Context.Authentication.SignIn(identity);
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                context.Rejected();
                context.SetError(OAuth2Constants.Errors.UnauthorizedClient, ex.Message);
            }

            return Task.FromResult(0);
        }
    }
}