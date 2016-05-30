﻿using System;
using System.Linq;

using AFT.RegoV2.Infrastructure.OAuth2;

using DotNetOpenAuth.OAuth2;

using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.Classes
{
    public class GameProviderOAuthStore : IOAuth2ClientStore
    {
        private readonly IRepository _repository;

        public GameProviderOAuthStore(IRepository repository)
        {
            _repository = repository;
        }


        public IClientDescription GetClient(string clientId, Guid gameProviderId)
        {
            var client = _repository.GameProviderConfigurations.FirstOrDefault(x => x.AuthorizationClientId == clientId);

            if (client == null || client.GameProviderId != gameProviderId)
                return null;

            return new OAuth2Client(clientId, client.AuthorizationSecret);
        }
    }


    public class OAuth2Client : IClientDescription
    {
        private readonly string clientIdentifier;
        private readonly string clientSecret;

        public string ClientName { get; set; }
        public string ClientIdentifier { get { return (clientIdentifier); } }

        /// <summary>Initialise a new instance of the FakeOAuth2Client based on the supplied identifier and secret.</summary>
        /// <param name="clientIdentifier">A unique ID that should match that supplied in the client_id field of client requests.</param>
        /// <param name="clientSecret">A secret shared between the client app and the authorization server.</param>
        public OAuth2Client(string clientIdentifier, string clientSecret)
        {
            this.clientIdentifier = clientIdentifier;
            this.clientSecret = clientSecret;
        }

        /// <summary>Determines whether a callback URI included in a client's authorization request is among those allowed callbacks for the registered client. This implementation always returns true.</summary>
        /// <param name="callback">The absolute URI the client has requested the authorization result be received at.  Never null.</param>
        /// <returns>
        /// <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>At the point this method is invoked, the identity of the client has <em>not</em>
        /// been confirmed.  To avoid open redirector attacks, the alleged client's identity
        /// is used to lookup a list of allowable callback URLs to make sure that the callback URL
        /// the actual client is requesting is one of the expected ones.</para>
        /// <para> From OAuth 2.0 section 2.1: 
        /// The authorization server SHOULD require the client to pre-register
        /// their redirection URI or at least certain components such as the
        /// scheme, host, port and path.  If a redirection URI was registered,
        /// the authorization server MUST compare any redirection URI received at
        /// the authorization endpoint with the registered URI.</para>
        /// </remarks>
        public bool IsCallbackAllowed(Uri callback)
        {
            return (true);
        }

        /// <summary>Checks whether the specified client secret is correct. This implementation returns true if the supplied secret matches <see cref="FakeOAuthClientStore.CLIENT_SECRET" /></summary>
        /// <param name="secret">The secret obtained from the client.</param>
        /// <returns><c>true</c> if the secret matches the one in the authorization server's record for the client; <c>false</c> otherwise.</returns>
        public bool IsValidClientSecret(string secret)
        {
            return (secret == clientSecret);
        }

        /// <summary>Gets the callback to use when an individual authorization request does not include an explicit callback URI.</summary>
        /// <remarks>Always returns null in this implementation.</remarks>
        public Uri DefaultCallback
        {
            get { return (null); }
        }

        /// <summary>Gets the type of the client.</summary>
        /// <remarks>Always returns ClientType.Confidential in this implementation.</remarks>
        public ClientType ClientType
        {
            get { return ClientType.Confidential; }
        }

        /// <summary>Get a value indicating whether we've registered a non-empty secret for this client.</summary>
        /// <remarks>This implementation always returns true since our demo client has a non-empty secret.</remarks>
        public bool HasNonEmptySecret
        {
            get { return (true); }
        }
    }
}
