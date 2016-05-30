using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.Shared.OAuth2;
using Newtonsoft.Json;

namespace AFT.RegoV2.Bonus.Api.Interface.Proxy
{
    public class OAuthProxy
    {
        private string _token;

        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Guid _actorId;

        protected OAuthProxy(HttpClient httpClient, string clientId, string clientSecret, Guid actorId)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _actorId = actorId;
        }

        public async Task<TResponse> SecureGet<TResponse>(string route, string query)
        {
            if (_token == null)
                _token = await GetToken();

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _token) },
                RequestUri = new Uri($"{route}?{query}", UriKind.Relative)
            };

            return await SendRequestMessage<TResponse>(message);
        }
        public async Task SecureGet(string route, string query)
        {
            if (_token == null)
                _token = await GetToken();

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _token) },
                RequestUri = new Uri($"{route}?{query}", UriKind.Relative)
            };

            await SendRequestMessage(message);
        }

        public async Task<TResponse> SecurePostAsJson<TRequest, TResponse>(string route, TRequest request)
        {
            if (_token == null)
                _token = await GetToken();

            var serializedRequest = JsonConvert.SerializeObject(request);
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _token) },
                RequestUri = new Uri(route, UriKind.Relative),
                Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json")
            };

            return await SendRequestMessage<TResponse>(message);
        }
        public async Task SecurePostAsJson<TRequest>(string route, TRequest request)
        {
            if (_token == null)
                _token = await GetToken();

            var serializedRequest = JsonConvert.SerializeObject(request);
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _token) },
                RequestUri = new Uri(route, UriKind.Relative),
                Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(message);
            if (response.IsSuccessStatusCode == false)
                await ThrowHttpException(response);
        }

        public async Task<string> GetToken()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.ClientCredentials},
                    { OAuth2Constants.ClientId, _clientId },
                    { OAuth2Constants.ClientSecret, _clientSecret }
                });
            content.Headers.Add("ActorId", _actorId.ToString());
            var response = await _httpClient.PostAsync(Routes.Token, content);
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();
                return tokenResponse.AccessToken;
            }

            var details = await response.Content.ReadAsAsync<UnauthorizedDetails>();
            throw new HttpException((int)response.StatusCode, details?.error_description ?? response.ReasonPhrase);
        }

        private async Task<TResponse> SendRequestMessage<TResponse>(HttpRequestMessage message)
        {
            var response = await _httpClient.SendAsync(message);
            if (response.IsSuccessStatusCode == false)
                await ThrowHttpException(response);

            return await response.Content.ReadAsAsync<TResponse>();
        }
        private async Task SendRequestMessage(HttpRequestMessage message)
        {
            var response = await _httpClient.SendAsync(message);
            if (response.IsSuccessStatusCode == false)
                await ThrowHttpException(response);
        }

        private async Task ThrowHttpException(HttpResponseMessage response)
        {
            var errorMessage = response.StatusCode == HttpStatusCode.BadRequest
                ? await response.Content.ReadAsStringAsync()
                : response.ReasonPhrase;
            throw new HttpException((int)response.StatusCode, errorMessage);
        }
    }
}
